import { Component, OnInit } from '@angular/core';
import { TranslatorService } from '@core/translator/translator.service';
import { Observable, Subject } from 'rxjs';

import { EventHistoryData } from '../../../shared/models/event-history-data.model';
import { AccountManagementService } from '../account-management.service';

@Component({
    templateUrl: 'user-history.component.html',
    providers: [AccountManagementService, TranslatorService],

})
export class UserHistoryComponent implements OnInit {

    public historyData: EventHistoryData[];
    public loading: boolean;
    public total: number;
    public page: number = 1;
    public quantity: number = 10;

    constructor(private accountService: AccountManagementService, public translator: TranslatorService) {

    }

    ngOnInit() {
        this.loadResources();
    }


    public loadResources() {
        this.accountService.getLogs(this.page, this.quantity).subscribe(response => {
            this.historyData = response.collection
            this.total = response.total;
        });
    }


    parse(details: string) {
        return JSON.parse(details);
    }
}
