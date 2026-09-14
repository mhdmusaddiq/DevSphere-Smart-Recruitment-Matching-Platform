import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerContactRequest } from '../data-access/employer.models';

@Component({selector:'app-employer-contact-requests',standalone:true,imports:[RouterLink],templateUrl:'./employer-contact-requests.component.html',styleUrl:'./employer-contact-requests.component.css'})
export class EmployerContactRequestsComponent implements OnInit {
  private readonly api=inject(EmployerApiService);
  requests:EmployerContactRequest[]=[]; loading=true; actionId:string|null=null; errorMessage=''; successMessage='';
  ngOnInit(){this.load();}
  load(){this.loading=true;this.api.getEmployerContactRequests().subscribe({next:r=>{this.requests=r;this.loading=false;},error:()=>{this.errorMessage='Unable to load contact requests.';this.loading=false;}});}
  cancel(r:EmployerContactRequest){
    if(r.status!=='Pending'||this.actionId)return;
    this.actionId=r.id;this.errorMessage='';this.successMessage='';
    this.api.cancelContactRequest(r.id).subscribe({
      next:u=>{this.requests=this.requests.map(x=>x.id===u.id?u:x);this.actionId=null;this.successMessage='Contact request cancelled.';},
      error:(e:HttpErrorResponse)=>{this.actionId=null;this.errorMessage=e.error?.message||'Unable to cancel contact request.';if(e.status===409)this.load();}
    });
  }
}
