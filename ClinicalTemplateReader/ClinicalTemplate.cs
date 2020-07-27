using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ClinicalTemplateReader
{
    public class ClinicalTemplate
    {
        private Application _app;
        private string _imageServer;

        public List<Protocol> ClinicalProtocols { get; set; }
        public List<PlanTemplate> PlanTemplates { get; set; }
        public List<ObjectiveTemplate> ObjectiveTemplates { get; set; }
        /// <summary>
        /// Generates properties for clinical templates
        /// 1. ClinicalProtocols
        /// 2. ObjecitveTemplates
        /// 3. PlanTemplates
        /// </summary>
        /// <param name="ImageServer"></param>
        public ClinicalTemplate(string ImageServer, Application app)
        {
            _app = app;
            _imageServer = ImageServer;
            ClinicalProtocols = new List<Protocol>();
            PlanTemplates = new List<PlanTemplate>();
            ObjectiveTemplates = new List<ObjectiveTemplate>();
            BuildClinicalProtocols();
            BuildOptimizationTemplates();
            BuildPlanTemplates();
        }
        private void BuildClinicalProtocols()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Protocol));
            foreach (var file in Directory.GetFiles(Path.Combine("\\\\", _imageServer, "va_data$", "programdata", "vision", "templates", "protocol")).Where(x => Path.GetExtension(x.ToLower()).Contains("xml")))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    var temp_protocol = ((Protocol)serializer.Deserialize(reader));
                    //Console.WriteLine($"{(temp_protocol.Phases.First().ObjectiveTemplate?.Type==null?"empty objective":$"{temp_protocol.Phases.First().ObjectiveTemplate.Type}")}");
                    ClinicalProtocols.Add(temp_protocol);
                }
            }
        }

        private void BuildOptimizationTemplates()
        {
            ObjectiveTemplates.Clear();
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectiveTemplate));
            foreach (var file in Directory.GetFiles(Path.Combine("\\\\", _imageServer, "va_data$", "programdata", "vision", "templates", "objective")).Where(x => Path.GetExtension(x.ToLower()).Contains("xml")))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    var temp_template = ((ObjectiveTemplate)serializer.Deserialize(reader));
                    //if (temp_template.Preview.ApprovalStatus.Contains("Approved"))
                    //{
                    ObjectiveTemplates.Add(temp_template);
                    //}
                }
            }
        }
        private void BuildPlanTemplates()
        {
            PlanTemplates.Clear();
            XmlSerializer serializer = new XmlSerializer(typeof(PlanTemplate));
            foreach (var file in Directory.GetFiles(Path.Combine("\\\\", _imageServer, "va_data$", "programdata", "vision", "templates", "plan")).Where(x => Path.GetExtension(x.ToLower()).Contains("xml")))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    var temp_template = ((PlanTemplate)serializer.Deserialize(reader));
                    //if (temp_template.Preview.ApprovalStatus.Contains("Approved"))
                    //{
                    PlanTemplates.Add(temp_template);
                    //}
                }
            }
        }
        /// <summary>
        /// Gathers plan template approval statuses
        /// </summary>
        /// <param name="includeClinicalProtocols">true if clinical protocols should be included in statistics.</param>
        /// <returns></returns>
        public List<ApprovalStatistics> GetPlanTemplateApprovals(bool includeClinicalProtocols)
        {
            List<ApprovalStatistics> approvals = new List<ApprovalStatistics>();
            var temp_templates = new List<PlanTemplate>(PlanTemplates);
         
            if (includeClinicalProtocols)
            {
                foreach (var protocol in ClinicalProtocols)
                {
                    foreach (var phase in protocol.Phases)
                    {
                        //converts clinical protocol into a plan template and copies the phase to the plan template.
                        var temp_template = phase.PlanTemplate;
                        temp_template.Preview = protocol.Preview;
                        temp_templates.Add(temp_template);
                    }
                }
            }
            foreach (var protocolgroup in temp_templates.GroupBy(x => x.Preview.ApprovalStatus))
            {
                approvals.Add(new ApprovalStatistics
                {
                    ApprovalStatus = protocolgroup.Key,
                    Count = protocolgroup.Count()
                });
            }
            return approvals;
        }
        /// <summary>
        /// Get approval statistics for objective template
        /// </summary>
        /// <param name="includeClinicalProtocols">If true, includes objective templates in clinical protocls.</param>
        /// <returns></returns>
        public List<ApprovalStatistics> GetObjectiveTemplateApprovals(bool includeClinicalProtocols)
        {
            List<ApprovalStatistics> approvals = new List<ApprovalStatistics>();
            var temp_templates = new List<ObjectiveTemplate>(ObjectiveTemplates);
            if (includeClinicalProtocols)
            {
                foreach (var protocol in ClinicalProtocols)
                {
                    foreach (var phase in protocol.Phases)
                    {
                        if (phase.ObjectiveTemplate != null)
                        {
                            //converts clinical protocol into a objective template and copies the phase to the objective template.
                            var temp_template = phase.ObjectiveTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                        else
                        {
                            Console.WriteLine($"phase objective null - {protocol.Preview.ID}");
                        }
                    }
                }
            }
            foreach (var protocolgroup in temp_templates.GroupBy(x => x.Preview.ApprovalStatus))
            {
                approvals.Add(new ApprovalStatistics
                {
                    ApprovalStatus = protocolgroup.Key,
                    Count = protocolgroup.Count()
                });
            }
            return approvals;
        }
        /// <summary>
        /// Gets site statics on plan templates. Treatment site location information
        /// </summary>
        /// <param name="approvedOnly">Only includes approved templates</param>
        /// <param name="includeClinicalProcotols">If true includes plan templates in clinical protocols.</param>
        /// <returns></returns>
        public List<SiteStatistics> GetPlanTemplateSiteStatistics(bool approvedOnly, bool includeClinicalProcotols)
        {
            List<SiteStatistics> sites = new List<SiteStatistics>();
            var temp_templates = new List<PlanTemplate>(PlanTemplates);
            if (approvedOnly)
            {
                temp_templates = temp_templates.Where(x => x.Preview.ApprovalStatus.Contains("Approved")).ToList();
            }
            if (includeClinicalProcotols)
            {
                if (approvedOnly)
                {
                    foreach (var protocol in ClinicalProtocols.Where(x => x.Preview.ApprovalStatus.Contains("Approved")))
                    {
                        foreach (var phase in protocol.Phases)
                        {
                            //converts clinical protocol into a plan template and copies the phase to the plan template.
                            var temp_template = phase.PlanTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                    }
                }
                else
                {
                    foreach (var protocol in ClinicalProtocols)
                    {
                        foreach (var phase in protocol.Phases)
                        {
                            var temp_template = phase.PlanTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                    }
                }
            }
            foreach (var protocolgroup in temp_templates.GroupBy(x => x.Preview.TreatmentSite))
            {
                sites.Add(new SiteStatistics
                {
                    Site = protocolgroup.Key,
                    Count = protocolgroup.Count()
                });
            }
            return sites;
        }
        public List<SiteStatistics> GetObjectiveTemplateSiteStatistics(bool approvedOnly, bool includeClinicalProtocols)
        {
            List<SiteStatistics> sites = new List<SiteStatistics>();
            var temp_templates = new List<ObjectiveTemplate>(ObjectiveTemplates);
            if (approvedOnly)
            {
                temp_templates = temp_templates.Where(x => x.Preview.ApprovalStatus.Contains("Approved")).ToList();
            }
            if (includeClinicalProtocols)
            {
                if (approvedOnly)
                {
                    foreach (var protocol in ClinicalProtocols.Where(x => x.Preview.ApprovalStatus.Contains("Approved")))
                    {
                        foreach (var phase in protocol.Phases)
                        {
                            var temp_template = phase.ObjectiveTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                    }
                }
                else
                {
                    foreach (var protocol in ClinicalProtocols)
                    {
                        foreach (var phase in protocol.Phases)
                        {
                            //converts clinical protocol into a objective template and copies the phase to the objective template.
                            var temp_template = phase.ObjectiveTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                    }
                }
            }
            foreach (var protocolgroup in temp_templates.GroupBy(x => x.Preview.TreatmentSite))
            {
                sites.Add(new SiteStatistics
                {
                    Site = protocolgroup.Key,
                    Count = protocolgroup.Count()
                });
            }
            return sites;
        }
        /// <summary>
        /// Performs the optimization for a given plan based on an optimization template
        /// </summary>
        /// <param name="plan">ExternalPlanSetup on which the plan optimization will be perfomed.</param>
        /// <param name="optimizationTemplate">ObjectiveTemplate to gather objectives</param>
        /// <param name="doseUnit">Dose Unit of Eclipse system for converting objective template doses</param>
        /// <returns>A string with the structures included in the optimization and the structures in the optimization template but not found in the planning structure set.</returns>
        public string OptimizeFromObjectiveTemplate(ExternalPlanSetup plan, ObjectiveTemplate optimizationTemplate, DoseValue.DoseUnit doseUnit)
        {
            //check to see if normal tissue objective is used. If auto, generate the auto NTO
            if (optimizationTemplate.Helios.NormalTissueObjective.Auto)
            {
                plan.OptimizationSetup.AddAutomaticNormalTissueObjective(optimizationTemplate.Helios.NormalTissueObjective.Priority);
            }
            else
            {
                //if not auto NTO take parameters from template.
                if (optimizationTemplate.Helios.NormalTissueObjective.Use)
                {
                    plan.OptimizationSetup.AddNormalTissueObjective(optimizationTemplate.Helios.NormalTissueObjective.Priority,
                        optimizationTemplate.Helios.NormalTissueObjective.DistanceFromTargetBorder,
                        optimizationTemplate.Helios.NormalTissueObjective.StartDose,
                        optimizationTemplate.Helios.NormalTissueObjective.EndDose,
                        optimizationTemplate.Helios.NormalTissueObjective.FallOff);
                }
            }
            List<string> optimizer_string = new List<string>();
            List<string> not_found_string = new List<string>();
            foreach (var optimizationObjective in optimizationTemplate.ObjectivesAllStructures)
            {
                if (plan.StructureSet.Structures.Any(x => x.Id == optimizationObjective.ID))
                {

                    var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == optimizationObjective.ID);
                    optimizer_string.Add(structure.Id);
                    foreach (var objective in optimizationObjective.StructureObjectives)
                    {
                        if (objective.Type == ObjectiveTypeEnum.Point)
                        {
                            plan.OptimizationSetup.AddPointObjective(structure,
                                objective.Operator == ObjectiveOperatorEnum.Upper ? OptimizationObjectiveOperator.Upper : OptimizationObjectiveOperator.Lower,
                                doseUnit == DoseValue.DoseUnit.cGy ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy) : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                                (double)objective.Volume,
                                (double)objective.Priority);
                        }
                        else if (objective.Type == ObjectiveTypeEnum.Mean)
                        {
                            plan.OptimizationSetup.AddMeanDoseObjective(structure,
                               doseUnit == DoseValue.DoseUnit.cGy ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy) : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                                (double)objective.Priority
                                );
                        }
                        else if (objective.Type == ObjectiveTypeEnum.GEUDs)
                        {
                            plan.OptimizationSetup.AddEUDObjective(structure,
                                objective.Operator == ObjectiveOperatorEnum.Lower ? OptimizationObjectiveOperator.Lower :
                                objective.Operator == ObjectiveOperatorEnum.Upper ? OptimizationObjectiveOperator.Upper :
                                objective.Operator == ObjectiveOperatorEnum.Target ? OptimizationObjectiveOperator.Exact :
                                OptimizationObjectiveOperator.None,
                                doseUnit == DoseValue.DoseUnit.cGy ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy) : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                                (double)objective.ParameterA,
                                (double)objective.Priority);
                        }
                        else
                        {
                            optimizer_string.Add("ESAPI cannot add line objectives.");
                        }
                    }
                }
                else
                {
                    not_found_string.Add(optimizationObjective.ID);
                }

            }
            if (plan.Beams.FirstOrDefault(x => !x.IsSetupField).GantryDirection != GantryDirection.None)
            {
                plan.OptimizeVMAT();
            }
            else
            {
                if (optimizationTemplate.Helios.MaxIterations != 0)
                {
                    plan.Optimize(optimizationTemplate.Helios.MaxIterations,
                        OptimizationOption.ContinueOptimizationWithPlanDoseAsIntermediateDose);
                }
            }
            return $"Structures Included in Optimization: {String.Join(", ", optimizer_string)}\nStructure not found on Structure Set for Optimiztion: {String.Join(", ", not_found_string)}";

        }


        /// <summary>
        /// Generate plan from plan template.
        /// Attempts to set the isocenter automatically. If needed, attempts to set as target center
        /// Target determined based on TargetId input, but can be null to use template.
        /// </summary>
        /// <param name="course">(optional) Course where plan should be created (null will generate a new course)</param>
        /// <param name="structureSet">StructureSet where plan should be created</param>
        /// <param name="planTemplate">Plan Template for plan creation</param>
        /// <param name="TargetId">(optional) Id of target structure of (null to use template structure)</param>
        /// <returns>The newly generated Plan setup</returns>
        public ExternalPlanSetup GeneratePlanFromTemplate(Course course, StructureSet structureSet, PlanTemplate planTemplate, string TargetId)
        {
            ////generate plan from template. 
            if (course == null)
            {
                course = structureSet.Patient.AddCourse();
                course.Id = "Auto" + (course.Patient.Courses.Any(x => x.Id.Contains("Auto")) ? (Convert.ToInt16(course.Patient.Courses.Where(x => x.Id.Contains("Auto")).OrderBy(x => x.Id).Last().Id.Last().ToString()) + 1).ToString() : "1");
            }

            var CurrentPlan = course.AddExternalPlanSetup(structureSet);
            int beamCount = 0;
            foreach (var field in planTemplate.Fields.Where(x => !x.Setup))
            {
                //planning for arc fields
                if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("ARC"))
                {
                    //first check if there is an MLC margin.
                    //If there is an MLC margin, a field needs to be set up so that the MLC margin
                    //can be set throughout the arc trajectory at a sampled control point spacing.
                    //This is done with a conformal arc beam with CP sampling of 20 degrees. 
                    if (field.MLCPlans.FirstOrDefault()?.MLCMargin?.Left != null)
                    {
                        var beam = CurrentPlan.AddConformalArcBeam(
                      new ExternalBeamMachineParameters(field.TreatmentUnit,
                      GetEnergyFromBeamTemplate(field.Energy),
                      field.DoseRate,
                      field.Technique,
                      field.PrimaryFluenceMode),
                      field.Collimator.Rtn,
                      20,
                      field.Gantry.Rtn,
                      (double)field.Gantry.StopRtn,
                      GetGantryDirection(field.Gantry.RtnDirection),
                      (double)field.TableRtn,
                      GetIsocenterFromTemplate(field, CurrentPlan, TargetId));
                        beam.Id = field.ID;
                        beam.FitMLCToStructure(new FitToStructureMargins(
                            ((double)field.FieldMargin.Left),
                            field.FieldMargin.Bottom == null ? (double)field.FieldMargin.Left : ((double)field.FieldMargin.Bottom),
                            field.FieldMargin.Right == null ? (double)field.FieldMargin.Left : ((double)field.FieldMargin.Right),
                            field.FieldMargin.Top == null ? (double)field.FieldMargin.Left : ((double)field.FieldMargin.Top)),
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == TargetId) ??
                             (CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID) ??
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == field.Target.StructureCode.Code) ??
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV")),
                            field.MLCPlans.FirstOrDefault().MLCMargin.OptimizeCollRtnFlag,
                            field.MLCPlans.FirstOrDefault().MLCMargin.JawFittingMode == "0" ? JawFitting.None : JawFitting.FitToRecommended,
                            GetLeafMeetingPoint(field.MLCPlans.FirstOrDefault().ContourMeetPoint),
                            GetClosedLeafMeetingPoint(field.MLCPlans.FirstOrDefault().ClosedMeetPoint));
                        //check the field fit to see if the jaws are wider than 15cm. 
                        if (beam.ControlPoints.FirstOrDefault().JawPositions.X2 - beam.ControlPoints.FirstOrDefault().JawPositions.X1 > 150)
                        {
                            beamCount = FitJawToVMATLimit(beamCount, beam);
                        }
                    }
                    else if (field.FieldMargin.Left != null)//margin could be set on the jaw rather than the MLC.
                    {
                        var beam = CurrentPlan.AddConformalArcBeam(
                        new ExternalBeamMachineParameters(field.TreatmentUnit,
                        GetEnergyFromBeamTemplate(field.Energy),
                        field.DoseRate,
                        field.Technique,
                        field.PrimaryFluenceMode),
                        field.Collimator.Rtn,
                        20,
                        field.Gantry.Rtn,
                        (double)field.Gantry.StopRtn,
                        GetGantryDirection(field.Gantry.RtnDirection),
                        (double)field.TableRtn,
                        GetIsocenterFromTemplate(field, CurrentPlan, TargetId));
                        beam.Id = field.ID;
                        beam.FitCollimatorToStructure(new FitToStructureMargins(
                            field.FieldMargin.Left == null ? 0.0 : ((double)field.FieldMargin.Left),
                            field.FieldMargin.Bottom == null ? 0.0 : ((double)field.FieldMargin.Bottom),
                            field.FieldMargin.Right == null ? 0.0 : ((double)field.FieldMargin.Right),
                            field.FieldMargin.Top == null ? 0.0 : ((double)field.FieldMargin.Top)),
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == TargetId) == null ?
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID) == null ?
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == field.Target.StructureCode.Code) == null ?
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV") :
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == field.Target.StructureCode.Code) :
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID) :
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == TargetId),
                            field.Collimator.Mode.Contains("X"),
                            field.Collimator.Mode.Contains("Y"),
                            field.FieldMargin.OptimizeCollRtnFlag);
                        if (beam.ControlPoints.FirstOrDefault().JawPositions.X2 - beam.ControlPoints.FirstOrDefault().JawPositions.X1 > 150)
                        {
                            beamCount = FitJawToVMATLimit(beamCount, beam);
                        }
                    }
                    else
                    {
                        //if no fitting, a simple arc beam can be used (only 2 control points in arc beam).
                        var beam = CurrentPlan.AddArcBeam(
                      new ExternalBeamMachineParameters(field.TreatmentUnit,
                      GetEnergyFromBeamTemplate(field.Energy),
                      field.DoseRate,
                      field.Technique,
                      field.PrimaryFluenceMode),
                      new VRect<double>(field.Collimator.X1 * 10.0,
                      field.Collimator.Y1 * 10.0,
                      field.Collimator.X2 * 10.0,
                      field.Collimator.Y2 * 10.0),
                      field.Collimator.Rtn,
                      field.Gantry.Rtn,
                      (double)field.Gantry.StopRtn,
                      GetGantryDirection(field.Gantry.RtnDirection),
                      (double)field.TableRtn,
                      GetIsocenterFromTemplate(field, CurrentPlan, TargetId));
                        beam.Id = field.ID;
                    }
                }
                else if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("IMRT"))//IMRT settings.
                {
                    //Static beam can be added because IMRT optimization will generate the MLC and sequences. 
                    var b = CurrentPlan.AddStaticBeam(
                        new ExternalBeamMachineParameters(field.TreatmentUnit,
                        GetEnergyFromBeamTemplate(field.Energy),
                        field.DoseRate,
                        field.Technique,
                        field.PrimaryFluenceMode),
                        new VRect<double>(field.Collimator.X1 * 10.0,
                        field.Collimator.Y1 * 10.0,
                        field.Collimator.X2 * 10.0,
                        field.Collimator.Y2 * 10.0),
                        field.Collimator.Rtn,
                        field.Gantry.Rtn,
                        (double)field.TableRtn,
                        GetIsocenterFromTemplate(field, CurrentPlan, TargetId));
                    b.Id = field.ID;
                    if (field.FieldMargin.BEVMarginFlag)
                    {
                        b.FitCollimatorToStructure(new FitToStructureMargins(
                            field.FieldMargin.Left == null ? 0.0 : (double)field.FieldMargin.Left,
                            field.FieldMargin.Bottom == null ? 0.0 : (double)field.FieldMargin.Bottom,
                            field.FieldMargin.Right == null ? 0.0 : (double)field.FieldMargin.Right,
                            field.FieldMargin.Top == null ? 0.0 : (double)field.FieldMargin.Top),
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == TargetId) == null ?
                             CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID) == null ?
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV") :
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID) :
                            CurrentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == TargetId),
                            field.Collimator.Mode.Contains("X"),
                            field.Collimator.Mode.Contains("Y"),
                            field.FieldMargin.OptimizeCollRtnFlag);
                    }
                }
                else if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("CONFROMAL"))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new ApplicationException("Could not determine beam type");
                }
            }
            //PlanResult = $"Generate Plan {CurrentPlan.Id} with the following fields:\nBeam Id\tGantry Angle\tTechnique\n{String.Join("\n", CurrentPlan.Beams.Select(x => new { s = $"{x.Id}\t{x.ControlPoints.First().GantryAngle}\t{x.Technique}" }).Select(x => x.s))}";
            return CurrentPlan;
        }

        private static int FitJawToVMATLimit(int beamCount, Beam beam)
        {
            //bring the first jaw in 10cm
            //bring the second jaw in to make 150mm.
            var editables = beam.GetEditableParameters();
            double x1 = beam.ControlPoints.First().JawPositions.X1;
            double x2 = beam.ControlPoints.First().JawPositions.X2;
            double fsx = x2 - x1;
            editables.SetJawPositions(new VRect<double>(
                beamCount % 2 == 0 ? x1 + 10.0 : x1 + (fsx - 150) - 10.0,
                beam.ControlPoints.First().JawPositions.Y1,
                beamCount % 2 == 0 ? x2 - (fsx - 150) + 10.0 : x2 - 10.0,
                beam.ControlPoints.First().JawPositions.Y2));
            beam.ApplyParameters(editables);
            beamCount++;
            return beamCount;
        }
        /// <summary>
        /// Converts template closed leaf meeting point to ESAPI closed leaf meeting point.
        /// </summary>
        /// <param name="closedMeetPoint">Leaf meeting point as defined in the clinical template.</param>
        /// <returns></returns>
        private ClosedLeavesMeetingPoint GetClosedLeafMeetingPoint(string closedMeetPoint)
        {
            if (closedMeetPoint.Contains("Center"))
            {
                return ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_Center;
            }
            else if (closedMeetPoint.Contains("A"))
            {
                return ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_BankOne;
            }
            else if (closedMeetPoint.Contains("B"))
            {
                return ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_BankTwo;
            }
            else
            {
                return ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_Center;
            }
        }
        /// <summary>
        /// Converts open leaf meeting point in template to Eclipse OpenLeavesMeetingPoint enum.
        /// </summary>
        /// <param name="contourMeetingPoint"></param>
        /// <returns></returns>
        private OpenLeavesMeetingPoint GetLeafMeetingPoint(string contourMeetingPoint)
        {
            if (contourMeetingPoint.Contains("Middle"))
            {
                return OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Middle;
            }
            else if (contourMeetingPoint.Contains("In"))
            {
                return OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Inside;
            }
            else if (contourMeetingPoint.Contains("Out"))
            {
                return OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Outside;
            }
            else
            {
                return OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Middle;
            }
        }
        /// <summary>
        /// Converts templated isocenter to isocenter in ESAPI coordinates. 
        /// </summary>
        /// <param name="beam">Beam where the iscoenter will be placed</param>
        /// <param name="plan">plan with the beam</param>
        /// <param name="TargetId">Target ID override. If this is set, the target is assumed to have this id.</param>
        /// <returns>VVector for isocenter position. </returns>
        private VVector GetIsocenterFromTemplate(Field beam, ExternalPlanSetup plan, string TargetId)
        {
            double x = Convert.ToDouble(beam.Isocenter.X);
            double y = Convert.ToDouble(beam.Isocenter.Y);
            double z = Convert.ToDouble(beam.Isocenter.Z);
            //if beam is relative to field target.
            if (beam.Isocenter.Placement == IscoenterPlacementEnum.AFTS || beam.Isocenter.Placement == IscoenterPlacementEnum.RFTS)
            {
                Structure target = null;
                //check for a structure with the ID TargetId first.
                if (!String.IsNullOrEmpty(TargetId))
                {
                    target = plan.StructureSet.Structures.FirstOrDefault(o => o.Id == TargetId);
                }
                if (target == null)
                {
                    //check the template's target volume ID second.
                    target = plan.StructureSet.Structures.FirstOrDefault(o => o.Id == beam.Target.VolumeID);
                    if (target == null)
                    {
                        //check for any PTV third.
                        target = plan.StructureSet.Structures.FirstOrDefault(o => o.DicomType.Contains("PTV"));
                        if (target == null) { throw new ApplicationException("Could not determine target"); }
                    }
                }
                var center = target.CenterPoint;
                //AFTS is at field target center.
                if (beam.Isocenter.Placement == IscoenterPlacementEnum.AFTS)
                {
                    return center;
                }
                else
                {
                    //RFTS is relative to field target center.
                    return new VVector(center.x + x, center.y + y, center.z + z);
                }
            }
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.AIO)//at image (user) origin
            {
                return plan.StructureSet.Image.UserOrigin;
            }
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.RIO)//relative to image (user) origin
            {
                var uo = plan.StructureSet.Image.UserOrigin;
                return new VVector(uo.x + x, uo.y + y, uo.z + z);
            }
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.AIC)//At image center.
            {
                return plan.StructureSet.Image.Origin;
            }
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.RIC)//Relative to image center.
            {
                var io = plan.StructureSet.Image.Origin;
                return new VVector(io.x + x, io.y + y, io.z + z);
            }
            else
            {
                //the default position will be the user origin.
                return plan.StructureSet.Image.UserOrigin;

            }
        }
        /// <summary>
        /// Converts template gantry direction to ESAPI gantry direction.
        /// </summary>
        /// <param name="rtnDirection"></param>
        /// <returns></returns>
        private GantryDirection GetGantryDirection(string rtnDirection)
        {
            switch (rtnDirection)
            {
                case "CW":
                    return GantryDirection.Clockwise;
                case "CC":
                    return GantryDirection.CounterClockwise;
                case "NONE":
                    return GantryDirection.None;
                default:
                    return GantryDirection.None;
            }
        }
        /// <summary>
        /// Converts template energy to ESAPI energy
        /// </summary>
        /// <param name="energy"></param>
        /// <returns>String with energy for external beam machine paramters</returns>
        private string GetEnergyFromBeamTemplate(EnergyModel energy)
        {
            return $"{Convert.ToInt32(energy.EnergyKV) / 1000:F0}{energy.Type}";
        }
        /// <summary>
        /// Converts template Prescription class to an Rx for an ESAPI PlanSetup
        /// </summary>
        /// <param name="planTemplate"></param>
        /// <param name="planSetup">Applies the Rx directly to this plan setup.</param>
        /// <param name="doseUnit"></param>
        public void SetRx(PlanTemplate planTemplate, PlanSetup planSetup, string doseUnit)
        {
            planSetup.SetPrescription((int)planTemplate.FractionCount, new DoseValue(doseUnit == "cGy" ? (double)planTemplate.DosePerFraction * 100.0 : (double)planTemplate.DosePerFraction, (doseUnit == "cGy" ? DoseValue.DoseUnit.cGy : DoseValue.DoseUnit.Gy)), (double)planTemplate.PrescribedPercentage);
        }


    }
}
