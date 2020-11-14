# Use and Description
The following is an example of using the ClinicalTemplateReder for automated planning. 
Start by generated a Stand-alone executable with the Eclipse Script Wizard

![Script Wizard](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/ScriptWizard.JPG)

## Save your solution!
Change your target framework to **.NET 4.5.2.**
Go to the Nuget Package Manager and find the **ClinicalTemplateReader** package. You may find this by searching ESAPI. 

![Nuget](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/NugetPackage.JPG)
## Review Template Statistics
In the Execute Method post the following code 

```
        static void Execute(Application app)
        {
            // TODO: Add your code here.
            //Checking clinical templates
            Console.WriteLine("Reading Clinical Templates");
            string server = "Varian Image Server";
            var clinicalProtols = new ClinicalTemplate(server);
            Console.WriteLine("Plan Template statistics with clinical protocols");
            var planStats_cp = clinicalProtols.GetPlanTemplateApprovals(true);
            foreach (var planstat in planStats_cp)
            {
                Console.WriteLine($"\t{planstat.ApprovalStatus} - {planstat.Count}");
            }
            var objStats_cp = clinicalProtols.GetObjectiveTemplateApprovals(true);
            Console.WriteLine("Objective Template Statistics with clinical protocols");
            foreach (var objstat in objStats_cp)
            {
                Console.WriteLine($"\t{objstat.ApprovalStatus} - {objstat.Count}");
            }
            Console.ReadLine();
        }
```

Running this should give the following response in the console.

![Template Summary](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/ApprovedTemplates.JPG)

## Selecting a plan template
The following code will allow you to select a plan template.

```
//Generating a plan from template.
            var appr_templates = clinicalProtols.PlanTemplates.Where(x => x.Preview.ApprovalStatus.Contains("Approved")).ToList();
            for (int i = 0; i < appr_templates.Count(); i++)
            {
                Console.WriteLine($"[{i}]. {appr_templates.ElementAt(i).Preview.ID} - {appr_templates.ElementAt(i).Preview.LastModified}");
            }
            Console.WriteLine($"\n\nPlease pick your plan template from the list (0 - {appr_templates.Count() - 1}):");
            var plantemplatenumber = Convert.ToInt32(Console.ReadLine());
            var plantemplate = appr_templates.ElementAt(plantemplatenumber);
            Console.WriteLine("\nObjective Template List (Approved):");
            //Get template description
            Console.WriteLine($"\n\nDescription of plan template");
            Console.WriteLine($"Number of Fields: {plantemplate.Fields.Count()}");
            Console.WriteLine($"Plan Rx: {plantemplate.DosePerFraction} x {plantemplate.FractionCount}");
            Console.ReadLine();
```

The result:

![Select Template](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/PickATemplate.JPG)

## Generating a plan from a template.
Finally, the following code will utilize the ClinicalTemplateReader API to generate an automated plan.

```
 //Generate plan from template
            var _patient = app.OpenPatientById("PatientIDHere");
            var _structureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == "ROI");
            Console.WriteLine("\n\nGenerating new automation plan");
            _patient.BeginModifications();
            //Generate plan
            var _newplan = clinicalProtols.GeneratePlanFromTemplate(null, _structureSet, plantemplate,null);
            //Set Prescription.
            clinicalProtols.SetRx(plantemplate, _newplan, "cGy");
            string plan_update = $"Plan created with {_newplan.Beams.Count()} beams";
            Console.Write($"Generated Plan {_newplan.Id} in {_newplan.Course.Id}\n{plan_update}\nPlan Rx: {_newplan.TotalDose} in {_newplan.NumberOfFractions}fx");
            app.SaveModifications();
            app.ClosePatient();
            Console.ReadLine();
```

The response from the console should be as follows. 

![PlanResponse](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/PlanGenerated.JPG)

After refreshing the patient in Eclipse, you should see a new course, plan, and fields generated on the patient.

![NewPlan](https://github.com/WUSTL-ClinicalDev/ClinicalTemplateReader/blob/master/ClinicalTemplateReader/DescriptionImages/NewPlanGenerated.JPG)

