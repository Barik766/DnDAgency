import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { appConfig } from './app.config';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering()
    // Removed withRoutes(serverRoutes) — this might have been the cause of the issue
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);