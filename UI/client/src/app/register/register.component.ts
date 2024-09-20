import { Component, inject, OnInit, output } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { NgIf } from '@angular/common';
import { TextInputComponent } from "../forms/text-input/text-input.component";
import { DatePickerComponent } from "../forms/date-picker/date-picker.component";
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, TextInputComponent, DatePickerComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit{
  private accountService = inject(AccountService);
  private router = inject(Router);
  registerForm: FormGroup = new FormGroup({});
  cancelRegister = output<boolean>();
  maxDate = new Date();
  validationErrors: string[] | undefined;

  register(){
    const dob = this.getDateOnly(this.registerForm.get('dateOfBirth')?.value);
    this.registerForm.patchValue({dateOfBirth: dob});
    this.accountService.register(this.registerForm.value).subscribe({
      next: _ => this.router.navigateByUrl('/members'),
      error: error => this.validationErrors = error
    })
  }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear())
  }

  initializeForm(){
    this.registerForm = new FormGroup({
      gender: new FormControl('male'),
      username: new FormControl('',[Validators.required]),
      knownAs: new FormControl('', Validators.required),
      city: new FormControl('', Validators.required),
      country: new FormControl('', Validators.required),
      dateOfBirth: new FormControl('', Validators.required),
      password: new FormControl('',[Validators.minLength(6), Validators.maxLength(8)]),
      confirmPassword: new FormControl('',[Validators.required, this.matchValues('password')]),
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: ()=> this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control : AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {isMatching: true}
    }
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

  getDateOnly(dob: string | undefined){
    if(!dob) return;
    return new Date(dob).toISOString().slice(0,10);
  }

}
