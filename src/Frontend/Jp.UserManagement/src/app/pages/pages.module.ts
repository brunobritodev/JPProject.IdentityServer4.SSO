import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { environment } from '@env/environment';
import { TranslateModule } from '@ngx-translate/core';
import { InputValidationComponent } from '@shared/components/input-validation/input-validation';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { GlobalConfigurationService } from '@shared/services/global-configuration.service';
import { RECAPTCHA_V3_SITE_KEY, RecaptchaFormsModule, RecaptchaModule, RecaptchaV3Module } from 'ng-recaptcha';
import { AlertModule } from 'ngx-bootstrap/alert';
import { NgxMaskModule } from 'ngx-mask';

import { UserService } from '../shared/services/user.service';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { P404Component } from './error/404.component';
import { P500Component } from './error/500.component';
import { LoginCallbackComponent } from './login-callback/login-callback.component';
import { LoginComponent } from './login/login.component';
import { RecoverComponent } from './recover/recover.component';
import { RegisterComponent } from './register/register.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';

const routes: Routes = [
    { path: "login", component: LoginComponent, data: { title: "Login Page" } },
    { path: "register", component: RegisterComponent, data: { title: "Register" } },
    { path: "login-callback", component: LoginCallbackComponent, data: { title: "Login" } },
    { path: "unauthorized", component: UnauthorizedComponent, data: { title: "Unauthorized" } },
    { path: "recover", component: RecoverComponent, data: { title: "Recover account" } },
    { path: "reset-password", component: ResetPasswordComponent, data: { title: "Reset password" } },
    { path: "confirm-email", component: ConfirmEmailComponent, data: { title: "Confirm account" } },
    { path: "404", component: P404Component, data: { title: "Not Found" } },
    { path: "500", component: P500Component, data: { title: "Error" } },
];


@NgModule({
    imports: [
        RouterModule.forRoot(routes),
        FormsModule,
        CommonModule,
        TranslateModule,
        ReactiveFormsModule,
        AlertModule.forRoot(),
        NgxMaskModule.forRoot(),
        RecaptchaModule,
        RecaptchaFormsModule,
    ],
    providers: [
        UserService,
        GlobalConfigurationService
    ],
    declarations: [
        LoginComponent,
        RegisterComponent,
        UnauthorizedComponent, 
        LoginCallbackComponent,
        RecoverComponent,
        ResetPasswordComponent,
        ConfirmEmailComponent,
        P404Component,
        P500Component,
        InputValidationComponent,
        LoadingSpinnerComponent
    ],
    exports: [
        RouterModule
    ]
})
export class PagesModule { }
