import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: any[];

  public constructor(titleService: Title,
    @Inject("isSearchEngine") private isSearchEngine: boolean,
    private http: HttpClient) {
    titleService.setTitle('Weather');
    console.info(`isSearchEngine: ${isSearchEngine}`); // in server side, it will be true if an agent is a SearchEngine crawler (See supply data in Startup.cs). In client side it will be false always, see main.ts
  }

  public async ngOnInit() {
    this.http.get<any[]>('odata/SampleApp/WeatherForecast').subscribe(result => {
      this.forecasts = result;
    });
  }
}
