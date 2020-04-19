import { Component } from '@angular/core';
import { AuthService } from '@core/auth/auth.service';
import { TranslatorService } from '@core/translator/translator.service';

@Component({
  selector: "app-dashboard",
  templateUrl: "unauthorized.component.html",
  providers: [TranslatorService]
})
export class UnauthorizedComponent {
    constructor(
        public authService: AuthService,
        public translator: TranslatorService){
        
    }

    public login() {
        this.authService.login();
    }
 }