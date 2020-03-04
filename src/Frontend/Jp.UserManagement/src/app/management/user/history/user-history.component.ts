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

    public addRow(index: number, tableName: string) {

        var table = document.getElementById(tableName) as HTMLTableElement;

        // Hide everyone before. In case users click in another item from list
        table.querySelectorAll("[temp='true']").forEach((i: HTMLTableRowElement) => i.remove());
        var item = this.historyData[index];

        if (item.show) {
            this.setEveryoneToNotShow();
            return;
        }

        let htmlContent = `<pre>${JSON.stringify(JSON.parse(item.details), null, 4)}</pre>`;

        // Create an empty <tr> element and add it to the 1st position of the table:
        var row = table.insertRow(index + 2);
        row.setAttribute("temp", "true")

        // Insert new cells (<td> elements) at the 1st and 2nd position of the "new" <tr> element:
        var cell = row.insertCell(0);
        // Add some text to the new cells:
        cell.innerHTML = htmlContent;
        cell.colSpan = 7;

        this.setEveryoneToNotShow();
        item.show = true;
    }

    private setEveryoneToNotShow() {
        // set all others items as show = false
        this.historyData.forEach(e => {
            e.show = false;
        });
    }
}
