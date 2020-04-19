import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '@env/environment';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { navItems } from '../../_nav';
import { AuthService } from '../auth/auth.service';
import { SettingsService } from '../settings/settings.service';
import { TranslatorService } from '../translator/translator.service';

@Component({
    selector: "app-dashboard",
    templateUrl: "./default-layout.component.html",
    providers: [SettingsService, TranslatorService]
})
export class DefaultLayoutComponent implements OnInit {

    public navItems = navItems;
    public sidebarMinimized = true;
    private changes: MutationObserver;
    public element: HTMLElement = document.body;
    public userProfile$: Observable<object>;
    constructor(public settingsService: SettingsService,
        public authService: AuthService,
        private router: Router,
        public translator: TranslatorService) {
        this.changes = new MutationObserver((mutations) => {
            this.sidebarMinimized = document.body.classList.contains("sidebar-minimized");
        });

        this.changes.observe(<Element>this.element, {
            attributes: true
        });

    }

    public ngOnInit() {
        this.getUserImage();
    }

    public logout() {
        this.authService.logout();
    }

    public setLanguage(value) {
        this.translator.useLanguage(value);
    }

    public async getUserImage() {
        this.userProfile$ = this.settingsService.getUserProfile()
            .pipe(
                tap(u => {
                    if (!environment.production)
                        console.table(u);
                }));
    }

}
