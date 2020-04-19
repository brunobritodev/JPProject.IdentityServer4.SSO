import { CommonModule } from '@angular/common';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { GlobalConfigurationService } from '@shared/services/global-configuration.service';
import {
    AuthConfig,
    JwksValidationHandler,
    OAuthModule,
    OAuthModuleConfig,
    OAuthStorage,
    ValidationHandler,
} from 'angular-oauth2-oidc';

import { authProdConfig } from './auth/auth-config.prod';
import { AuthGuardWithForcedLogin } from './auth/auth-guard-with-forced-login.service';
import { AuthGuard } from './auth/auth-guard.service';
import { authModuleConfig } from './auth/auth-module-config';
import { AuthService } from './auth/auth.service';
import { SettingsService } from './settings/settings.service';
import { TranslatorService } from './translator/translator.service';

export function storageFactory(): OAuthStorage {
    return localStorage;
}

@NgModule({
    imports: [
        CommonModule,
        OAuthModule.forRoot(),
    ],
    // declarations: [TitleComponent],
    // exports: [TitleComponent],
    providers: [
        TranslatorService,
        SettingsService,
        AuthService,
        AuthGuard,
        AuthGuardWithForcedLogin,
    ]
})
export class CoreModule {

    constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
        if (parentModule) {
            throw new Error('CoreModule is already loaded. Import it in the AppModule only');
        }
    }

    static forRoot(): ModuleWithProviders {
        return {
            ngModule: CoreModule,
            providers: [
                TranslatorService,
                SettingsService,
                { provide: AuthConfig, useValue: authProdConfig },
                { provide: OAuthModuleConfig, useValue: authModuleConfig },
                { provide: ValidationHandler, useClass: JwksValidationHandler },
                { provide: OAuthStorage, useFactory: storageFactory }
            ]
        };
    }
}