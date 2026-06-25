import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnalysisDetails } from './analysis-details';

describe('AnalysisDetails', () => {
  let component: AnalysisDetails;
  let fixture: ComponentFixture<AnalysisDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnalysisDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(AnalysisDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
