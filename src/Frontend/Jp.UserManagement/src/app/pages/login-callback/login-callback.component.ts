import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { TranslatorService } from '@core/translator/translator.service';
import { Subscription } from 'rxjs';

import { SettingsService } from '../../core/settings/settings.service';

@Component({
    selector: "app-dashboard",
    templateUrl: "login-callback.component.html",
    providers: [SettingsService, TranslatorService]
})
export class LoginCallbackComponent implements OnInit, OnDestroy {
    stream: Subscription;

    constructor(
        private authService: AuthService,
        private router: Router,
        public translator: TranslatorService) { }

    public ngOnInit() {
        this.stream = this.authService.canActivateProtectedRoutes$.subscribe(yes => {
            if (yes)
                return this.router.navigate(['/home']);
            else
                return this.router.navigate(['/login']);
        });
    }

    public ngOnDestroy() {
        this.stream.unsubscribe();
    }
}