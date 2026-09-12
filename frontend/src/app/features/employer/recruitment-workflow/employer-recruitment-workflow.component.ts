import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerInterview, EmployerOffer, EmployerWorkflowSummary } from '../data-access/employer.models';

@Component({selector:'app-employer-recruitment-workflow',standalone:true,imports:[CommonModule,ReactiveFormsModule],templateUrl:'./employer-recruitment-workflow.component.html',styleUrl:'./employer-recruitment-workflow.component.css'})
export class EmployerRecruitmentWorkflowComponent implements OnInit{
  private readonly api=inject(EmployerApiService);private readonly route=inject(ActivatedRoute);private readonly fb=inject(FormBuilder);
  applicationId='';summary:EmployerWorkflowSummary|null=null;loading=true;busy=false;errorMessage='';successMessage='';
  interview=this.fb.nonNullable.group({notes:['']});
  slot=this.fb.nonNullable.group({interviewId:['',Validators.required],starts:['',Validators.required],ends:['',Validators.required],location:['']});
  score=this.fb.group({interviewId:this.fb.control<string|null>(null),rating:this.fb.nonNullable.control(3,[Validators.min(1),Validators.max(5)]),notes:this.fb.nonNullable.control('')});
  offer=this.fb.group({salary:this.fb.control<number|null>(null,[Validators.min(0)]),expires:this.fb.control<string|null>(null),notes:this.fb.nonNullable.control('')});
  talent=this.fb.nonNullable.group({consent:[false,Validators.requiredTrue],notes:['']});
  ngOnInit(){this.applicationId=this.route.snapshot.paramMap.get('applicationId')??'';if(!this.applicationId){this.loading=false;this.errorMessage='Application identifier is missing.';return;}this.load();}
  load(){this.loading=true;this.api.getEmployerWorkflowSummary(this.applicationId).subscribe({next:s=>{this.summary=s;this.loading=false;},error:(e:HttpErrorResponse)=>{this.errorMessage=e.error?.message||'Unable to load recruitment workflow.';this.loading=false;}});}
  private run(o:any,msg:string){this.busy=true;this.errorMessage='';o.subscribe({next:()=>{this.busy=false;this.successMessage=msg;this.load();},error:(e:HttpErrorResponse)=>{this.busy=false;this.errorMessage=e.error?.message||'Workflow action failed.';}});}
  createInterview(){if(!this.busy)this.run(this.api.createInterview(this.applicationId,this.interview.controls.notes.value),'Interview created.');}
  setInterview(i:EmployerInterview,s:'Completed'|'Cancelled'){if(i.status==='Scheduled'&&!this.busy)this.run(this.api.updateInterviewStatus(i.id,s),`Interview ${s.toLowerCase()}.`);}
  addSlot(){if(this.slot.invalid||this.busy){this.slot.markAllAsTouched();return;}const v=this.slot.getRawValue();this.run(this.api.addInterviewSlot(v.interviewId,new Date(v.starts).toISOString(),new Date(v.ends).toISOString(),v.location),'Interview slot added.');}
  createScore(){if(this.score.invalid||this.busy)return;const v=this.score.getRawValue();this.run(this.api.createScorecard(this.applicationId,v.interviewId||null,v.rating,v.notes),'Scorecard created.');}
  createOffer(){if(this.offer.invalid||this.busy)return;const v=this.offer.getRawValue();this.run(this.api.createOffer(this.applicationId,v.salary,v.expires?new Date(v.expires).toISOString():null,v.notes),'Draft offer created.');}
  offerActions(o:EmployerOffer){return o.status==='Draft'?['Extended','Withdrawn']:o.status==='Extended'?['Accepted','Declined','Withdrawn']:[];}
  setOffer(o:EmployerOffer,s:'Extended'|'Accepted'|'Declined'|'Withdrawn'){if(!this.busy)this.run(this.api.updateOfferStatus(o.id,s),`Offer updated to ${s}.`);}
  addTalent(){if(this.talent.invalid||this.busy){this.talent.markAllAsTouched();return;}const v=this.talent.getRawValue();this.run(this.api.addTalentPoolEntry(this.applicationId,v.consent,v.notes),'Talent pool entry created.');}
}
