import { Component, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { EntityContextProvider } from '../bit-exports';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: SampleAppModel.WeatherForecastDto[];

  public constructor(titleService: Title, @Inject("isSearchEngine") private isSearchEngine: boolean) {
    titleService.setTitle('Weather');
    console.info(`isSearchEngine: ${isSearchEngine}`); // in server side, it will be true if an agent is a SearchEngine crawler (See supply data in Startup.cs). In client side it will be false always, see main.ts
  }

  public async ngOnInit() {
    if (typeof global == "undefined") {
      /* code is running client side */
      const entityContextProvider = EntityContextProvider;
      const context = await entityContextProvider.getContext<SampleAppContext>("SampleApp");
      this.forecasts = await context.weatherForecast.toArray();
    }
    else {
      // this is a sample of detecting client/server in code. You can use httpClient in server side too. Do not wrap all httpClient codes in such a if!
      this.forecasts = [new SampleAppModel.WeatherForecastDto({ Date: new Date(), TemperatureC: 12, TemperatureF: 20, Summary: '!' })];
    }
  }
}
