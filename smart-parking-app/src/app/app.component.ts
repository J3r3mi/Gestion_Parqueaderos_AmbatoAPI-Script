import { Component } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { addIcons } from 'ionicons';
import {
  carSport,
  logOutOutline,
  listOutline,
  navigateOutline,
  peopleOutline,
  refreshOutline,
  checkmarkCircle,
  lockOpenOutline,
  mailOutline,
  informationCircleOutline,
  gridOutline,
  qrCodeOutline,
  mapOutline,
  serverOutline,
  swapHorizontalOutline,
  timeOutline,
  alertCircleOutline,
  shieldCheckmarkOutline,
  flameOutline,
  notificationsOutline,
  cashOutline,
  speedometerOutline,
  businessOutline,
  layersOutline,
  locationOutline,
  chevronForwardOutline,
  scanOutline,
  expandOutline,
  contractOutline,
  chevronUpOutline,
  chevronDownOutline
} from 'ionicons/icons';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [IonicModule],
  template: `
    <ion-app>
      <ion-router-outlet></ion-router-outlet>
    </ion-app>
  `
})
export class AppComponent {
  constructor() {
    addIcons({
      carSport,
      logOutOutline,
      listOutline,
      navigateOutline,
      peopleOutline,
      refreshOutline,
      checkmarkCircle,
      lockOpenOutline,
      mailOutline,
      informationCircleOutline,
      gridOutline,
      qrCodeOutline,
      mapOutline,
      serverOutline,
      swapHorizontalOutline,
      timeOutline,
      alertCircleOutline,
      shieldCheckmarkOutline,
      flameOutline,
      notificationsOutline,
      cashOutline,
      speedometerOutline,
      businessOutline,
      layersOutline,
      locationOutline,
      chevronForwardOutline,
      scanOutline,
      expandOutline,
      contractOutline,
      chevronUpOutline,
      chevronDownOutline
    });
  }
}
