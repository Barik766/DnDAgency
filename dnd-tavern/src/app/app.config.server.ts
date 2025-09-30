import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { appConfig } from './app.config';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering()
    // Убрал withRoutes(serverRoutes) - это может быть причиной проблемы
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);