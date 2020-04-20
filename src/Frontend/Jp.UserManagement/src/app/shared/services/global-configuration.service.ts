import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@env/environment';
import { PublicSettings } from '@shared/view-model/public-settings.model';
import { Observable } from 'rxjs';

@Injectable()
export class GlobalConfigurationService {
    
    endpoint: string;

    constructor(private http: HttpClient) {
        this.endpoint = environment.ResourceServer + "global-configuration";
    }

    public publicSettings(): Observable<PublicSettings> {
        return this.http.get<PublicSettings>(`${this.endpoint}/public-settings`);
    }

}