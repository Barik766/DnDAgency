import { Component } from "@angular/core";
import { HeroSection } from "./components/hero-section/hero-section";
import { HowItWorksComponent } from "./components/how-it-works/how-it-works";
import { UpcomingGames } from "./components/upcoming-games/upcoming-games";
import { OurMastersComponent } from "./components/our-masters/our-masters";
import { NewsletterComponent } from "./components/newsletter/newsletter";

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [HeroSection, HowItWorksComponent, UpcomingGames, OurMastersComponent, NewsletterComponent]
})
export class HomeComponent {}
