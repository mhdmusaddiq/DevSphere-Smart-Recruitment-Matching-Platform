import { CommonModule } from '@angular/common';import { Component,inject,OnInit } from '@angular/core';
import { EmployerApiService } from '../data-access/employer-api.service';import { EmployerNotification } from '../data-access/employer.models';
@Component({selector:'app-employer-notifications',standalone:true,imports:[CommonModule],templateUrl:'./employer-notifications.component.html',styleUrl:'./employer-notifications.component.css'})
export class EmployerNotificationsComponent implements OnInit{
private readonly api=inject(EmployerApiService);notifications:EmployerNotification[]=[];loading=true;actionId:string|null=null;errorMessage='';
ngOnInit(){this.load();}get unread(){return this.notifications.filter(n=>!n.isRead).length;}
load(){this.loading=true;this.api.getNotifications().subscribe({next:n=>{this.notifications=n;this.loading=false;},error:()=>{this.errorMessage='Unable to load notifications.';this.loading=false;}});}
mark(n:EmployerNotification){if(n.isRead||this.actionId)return;this.actionId=n.id;this.api.markNotificationRead(n.id).subscribe({next:()=>{this.notifications=this.notifications.map(x=>x.id===n.id?{...x,isRead:true}:x);this.actionId=null;},error:()=>{this.errorMessage='Unable to mark notification as read.';this.actionId=null;}});}
}
