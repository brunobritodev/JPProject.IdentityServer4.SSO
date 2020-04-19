import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatorService } from '@core/translator/translator.service';
import { FormControl, FormGroup, Validators } from '@ng-stack/forms';
import { EqualToValidator, PasswordValidator } from '@shared/validators';
import { AuthService, FacebookLoginProvider, GoogleLoginProvider, VkontakteLoginProvider } from 'angular-6-social-login-v2';
import { ReCaptchaV3Service } from 'ng-recaptcha';
import { AlertConfig } from 'ngx-bootstrap/alert';
import { Observable, Subject, throwError as observableThrowError } from 'rxjs';
import { debounceTime, share, switchMap } from 'rxjs/operators';
import { FormUtil } from 'src/app/shared/validators/form.utils';

import { RegisterUser, User } from '../../shared/models/user.model';
import { UserService } from '../../shared/services/user.service';
import { ProblemDetails } from '../../shared/view-model/default-response.model';


export function getAlertConfig(): AlertConfig {
    return Object.assign(new AlertConfig(), { type: "success" });
}

@Component({
    selector: "app-dashboard",
    templateUrl: "register.component.html",
    providers: [
        UserService,
        TranslatorService,
        { provide: AlertConfig, useFactory: getAlertConfig },
        AuthService
    ]
})
export class RegisterComponent implements OnInit {

    readonly registerForm = new FormGroup<RegisterUser>({
        password: new FormControl<string>(null, [Validators.required, PasswordValidator.validator]),
        confirmPassword: new FormControl<string>(null, [Validators.required, EqualToValidator.validator('password')]),
        email: new FormControl<string>(null, [Validators.required, Validators.email]),
        name: new FormControl<string>(null, Validators.minLength(2)),
        username: new FormControl<string>(null, [Validators.required]),
        phoneNumber: new FormControl<string>(null, null),
        providerId: new FormControl<string>(null, null),
        picture: new FormControl<string>(null, null),
        provider: new FormControl<string>(null, null)
    });


    public errors: Array<string>;
    showButtonLoading: boolean;
    userExist: boolean;
    emailExist: boolean;
    public socialLoggedIn: boolean;


    constructor(
        private userService: UserService,
        private router: Router,
        private route: ActivatedRoute,
        private socialAuthService: AuthService,
        public translator: TranslatorService,
        private recaptchaV3Service: ReCaptchaV3Service) { }

    public ngOnInit() {
        this.errors = [];
        this.socialLoggedIn = false;
        // this.getLoginInfo();

        this.registerForm.controls.email.valueChanges.pipe(debounceTime(500))
            .pipe(switchMap(a => this.userService.checkEmail(a)))
            .subscribe((response: boolean) => {
                this.emailExist = response;
                if (this.emailExist)
                    this.registerForm.controls['email'].setErrors({ 'emailExist': true });
            });

        this.registerForm.controls.username.valueChanges.pipe(debounceTime(500))
            .pipe(switchMap(a => this.userService.checkUserName(a)))
            .subscribe((response: boolean) => {
                this.emailExist = response;
                if (this.emailExist)
                    this.registerForm.controls['username'].setErrors({ 'usernameExist': true });
            });
    }

    public async getLoginInfo() {
        // this.loginInfo = Object.setPrototypeOf((await this.accountService.getLoginInfo().toPromise()).data, LoginInfo.prototype);
    }

    public register() {

        if (!this.validateForm(this.registerForm)) {
            return;
        }

        this.recaptchaV3Service.execute('importantAction').subscribe((token) => {
            this.showButtonLoading = true;
            this.userService.register(this.registerForm.value, token).subscribe(
                registerResult => { if (registerResult) this.router.navigate(["/login"]); },
                err => {
                    this.errors = ProblemDetails.GetErrors(err).map(a => a.value);
                    this.showButtonLoading = false;
                }
            );
        });

        

    }

    validateForm(form) {
        if (form.invalid) {
            FormUtil.touchForm(form);
            FormUtil.dirtyForm(form);

            return false;
        }
        return true;
    }

    public getErrorMessages(): Observable<any> {
        return this.translator.translate.get('validations').pipe(share());
    }

    public socialSignIn(socialPlatform: string) {
        let socialPlatformProvider;
        if (socialPlatform === "facebook") {
            socialPlatformProvider = FacebookLoginProvider.PROVIDER_ID;
        } else if (socialPlatform === "google") {
            socialPlatformProvider = GoogleLoginProvider.PROVIDER_ID;
        }


        this.socialAuthService.signIn(socialPlatformProvider).then(
            (userData) => {
                this.socialLoggedIn = true;
                // Now sign-in with userData
                // ...
                this.registerForm.controls['email'].setValue(userData.email);
                this.registerForm.controls['name'].setValue(userData.name);
                this.registerForm.controls['picture'].setValue(userData.image);
                this.registerForm.controls['provider'].setValue(userData.provider);
                this.registerForm.controls['providerId'].setValue(userData.id);
            }
        );
    }
}
