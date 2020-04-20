import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SettingsService } from '@core/settings/settings.service';
import { environment } from '@env/environment';
import { Observable } from 'rxjs';

import { RegisterUser } from '../models/register-user';
import { User } from '../models/user.model';
import { ConfirmEmail } from '../view-model/confirm-email.model';
import { ProblemDetails } from '../view-model/default-response.model';
import { ForgotPassword } from '../view-model/forgot-password.model';
import { ResetPassword } from '../view-model/reset-password.model';

@Injectable()
export class UserService {

    endpoint: string;
    endpointUser: string;

    constructor(private http: HttpClient) {
        this.endpoint = environment.ResourceServer + "sign-up";
        this.endpointUser = environment.ResourceServer + "user";
    }
    public register(register: RegisterUser): Observable<RegisterUser> {
        return this.http.post<RegisterUser>(`${this.endpoint}`, register);
    }

    public registerWithCaptcha(register: RegisterUser, token: string): Observable<RegisterUser> {
        return this.http.post<RegisterUser>(`${this.endpoint}`, register, { headers: new HttpHeaders({ 'recaptcha': token }) });
    }

    public checkUserName(userName: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.endpoint}/check-username/${userName}`);
    }

    public checkEmail(email: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.endpoint}/check-email/${encodeURI(email)}`);
    }

    public recoverPassword(emailOrPassword: ForgotPassword): Observable<boolean> {
        return this.http.post<boolean>(`${this.endpointUser}/${encodeURI(emailOrPassword.usernameOrEmail)}/password/forget`, {});
    }

    public resetPassword(username: string, model: ResetPassword): any {
        return this.http.post<boolean>(`${this.endpointUser}/${encodeURI(username)}/password/reset`, model);
    }

    public confirmEmail(username: string, model: ConfirmEmail): any {
        return this.http.post<boolean>(`${this.endpointUser}/${encodeURI(username)}/confirm-email`, model);
    }
}
