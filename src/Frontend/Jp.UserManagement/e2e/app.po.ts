import { browser, element, by } from 'protractor';

export class CoreUIPage {
  navigateTo() {
    return browser.get('/');
  }

  getHeaderText() {
    return element(by.css('.app-body .card h1')).getText();
  }
}
