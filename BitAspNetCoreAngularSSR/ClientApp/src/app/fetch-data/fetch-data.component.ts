import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: WeatherForecast[];

  public constructor(titleService: Title, private http: HttpClient, @Inject("isSearchEngine") private isSearchEngine: boolean) {
    titleService.setTitle('Weather');
    console.info(`isSearchEngine: ${isSearchEngine}`); // in server side, it will be true if an agent is a SearchEngine crawler (See supply data in Startup.cs). In client side it will be false always, see main.ts
  }

  public ngOnInit() {
    if (typeof global == "undefined") {
    /* code is running client side */
      this.http.get<WeatherForecast[]>('odata/SampleApp/WeatherForecast').subscribe(result => {
        this.forecasts = result;
      }, error => console.error(error));
    }
    else {
      // this is a sample of detecting client/server in code. You can use httpClient in server side too. Do not wrap all httpClient codes in such a if!
      this.forecasts = [{ Date: new Date(), TemperatureC: 12, TemperatureF: 20, Summary:'!' }];
    }
  }
}

interface WeatherForecast {
  Date: Date;
  TemperatureC: number;
  TemperatureF: number;
  Summary: string;
}
