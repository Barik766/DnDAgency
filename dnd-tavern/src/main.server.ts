import { bootstrapApplication, BootstrapContext } from '@angular/platform-browser';
import { App } from './app/app';
import { config } from './app/app.config.server';

// New approach for Angular 20.3+ using BootstrapContext
const bootstrap = (context: BootstrapContext) => 
  bootstrapApplication(App, config, context);

export default bootstrap;