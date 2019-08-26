import 'zone.js/dist/zone-node';
import 'reflect-metadata';
import { renderModule, renderModuleFactory } from '@angular/platform-server';
import { APP_BASE_HREF } from '@angular/common';
import { enableProdMode } from '@angular/core';
import { provideModuleMap } from '@nguniversal/module-map-ngfactory-loader';
import { createServerRenderer } from 'aspnet-prerendering';
export { AppServerModule } from './app/app.server.module';

enableProdMode();

export default createServerRenderer(async params => {
  const { AppServerModule, AppServerModuleNgFactory, LAZY_MODULE_MAP } = module.exports;

  console.info(`isSearchEngine's value is: ${params.data.isSearchEngine}`);
  // you can use console.info and other console methods to see variable values in SSR. These info can be seen in asp.net core's console.
  // It's a good idea to use asp.net core in console mode instead of IIS Express.

  const options = {
    document: params.data.originalHtml,
    url: params.url,
    extraProviders: [
      provideModuleMap(LAZY_MODULE_MAP),
      { provide: APP_BASE_HREF, useValue: params.baseUrl },
      { provide: 'BASE_URL', useValue: params.origin + params.baseUrl },
      { provide: 'isSearchEngine', useValue: params.data.isSearchEngine }
    ]
  };

  // Bypass ssr api call cert warnings in development
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

  const renderPromise = AppServerModuleNgFactory
    ? /* AoT */ renderModuleFactory(AppServerModuleNgFactory, options)
    : /* dev */ renderModule(AppServerModule, options);

  const html = await renderPromise;

  return ({ html });
});
