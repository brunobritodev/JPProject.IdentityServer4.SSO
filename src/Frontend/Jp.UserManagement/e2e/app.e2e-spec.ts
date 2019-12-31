import { CoreUIPage } from './app.po';

describe('core-ui App', function() {
  let page: CoreUIPage;

  beforeEach(() => {
    page = new CoreUIPage();
  });

  it('should display card containing Login', () => {
    page.navigateTo();
    expect(page.getHeaderText()).toContain('Login');
  });
});
