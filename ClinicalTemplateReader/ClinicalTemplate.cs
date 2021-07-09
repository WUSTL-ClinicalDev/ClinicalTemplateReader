using ClinicalTemplateReader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ClinicalTemplateReader
{
    public class ClinicalTemplate
    {
        #region Variables & Contants

        /// <summary>
        /// Path of the protocol templates
        /// </summary>
        private static readonly string TEMPLATES_PROTOCOL_PATH = @"va_data$\programdata\vision\templates\protocol";

        /// <summary>
        /// Path of the objective templates
        /// </summary>
        private static readonly string TEMPLATES_OBJECTIVE_PATH = @"va_data$\programdata\vision\templates\objective";

        /// <summary>
        /// Path of the plan templates
        /// </summary>
        private static readonly string TEMPLATES_PLAN_PATH = @"va_data$\programdata\vision\templates\plan";

        /// <summary>
        /// List of the clinical protocols
        /// </summary>
        public List<Protocol> ClinicalProtocols { get; private set; }

        /// <summary>
        /// List of the plan templates
        /// </summary>
        public List<PlanTemplate> PlanTemplates { get; private set; }

        /// <summary>
        /// List of the objective templates
        /// </summary>
        public List<ObjectiveTemplate> ObjectiveTemplates { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Generates properties for clinical templates
        /// 1. ClinicalProtocols
        /// 2. ObjecitveTemplates
        /// 3. PlanTemplates
        /// </summary>
        /// <param name="imageServer">Address of the image server (e.g. hospImgSrv or 192.168.88.130)</param>
        public ClinicalTemplate(string imageServer)
        {
            ClinicalProtocols = DeserializeXmlFile<Protocol>(Path.Combine(@"\\", imageServer, TEMPLATES_PROTOCOL_PATH));
            ObjectiveTemplates = DeserializeXmlFile<ObjectiveTemplate>(Path.Combine(@"\\", imageServer, TEMPLATES_OBJECTIVE_PATH));
            PlanTemplates = DeserializeXmlFile<PlanTemplate>(Path.Combine(@"\\", imageServer, TEMPLATES_PLAN_PATH));
        }

        #endregion        

        #region Public APIs

        /// <summary>
        /// Gathers plan template approval statuses
        /// </summary>
        /// <param name="includeClinicalProtocols">True if clinical protocols should be included in statistics.</param>
        /// <returns>List of plan template approves statuses</returns>
        public List<ApprovalStatistics> GetPlanTemplateApprovals(bool includeClinicalProtocols)
        {
            var temp_templates = new List<PlanTemplate>(PlanTemplates);

            if (includeClinicalProtocols)
            {
                foreach (var protocol in ClinicalProtocols)
                {
                    foreach (var phase in protocol.Phases)
                    {
                        // Converts clinical protocol into a plan template and copies the phase to the plan template.
                        var temp_template = phase.PlanTemplate;
                        temp_template.Preview = protocol.Preview;
                        temp_templates.Add(temp_template);
                    }
                }
            }

            return GetApprovalStatistics<PlanTemplate>(temp_templates.GroupBy(x => x.Preview.ApprovalStatus));
        }        

        /// <summary>
        /// Get approval statistics for objective template
        /// </summary>
        /// <param name="includeClinicalProtocols">If true, includes objective templates in clinical protocols.</param>
        /// <returns>List of objective template approves statuses</returns>
        public List<ApprovalStatistics> GetObjectiveTemplateApprovals(bool includeClinicalProtocols)
        {
            var temp_templates = new List<ObjectiveTemplate>(ObjectiveTemplates);

            if (includeClinicalProtocols)
            {
                foreach (var protocol in ClinicalProtocols)
                {
                    foreach (var phase in protocol.Phases)
                    {
                        if (phase.ObjectiveTemplate != null)
                        {
                            // Converts clinical protocol into a objective template and copies the phase to the objective template.
                            var temp_template = phase.ObjectiveTemplate;
                            temp_template.Preview = protocol.Preview;
                            temp_templates.Add(temp_template);
                        }
                        else
                        {
                            Debug.WriteLine($"Phase Objective null - {protocol.Preview.ID}");
                        }
                    }
                }
            }

            return GetApprovalStatistics<ObjectiveTemplate>(temp_templates.GroupBy(x => x.Preview.ApprovalStatus));
        }        

        /// <summary>
        /// Gets site statics on plan templates. Treatment site location information.
        /// </summary>
        /// <param name="approvedOnly">Only includes approved templates</param>
        /// <param name="includeClinicalProcotols">If true includes plan templates in clinical protocols.</param>
        /// <returns>List of plan template site statistics</returns>
        public List<SiteStatistics> GetPlanTemplateSiteStatistics(bool approvedOnly, bool includeClinicalProcotols)
        {
            var temp_templates = new List<PlanTemplate>(PlanTemplates);
            if (approvedOnly)
            {
                temp_templates = temp_templates.Where(x => x.Preview.ApprovalStatus.Contains("Approved")).ToList();
            }

            if (includeClinicalProcotols)
            {
                var protocols = approvedOnly ? ClinicalProtocols : ClinicalProtocols.Where(x => x.Preview.ApprovalStatus.Contains("Approved"));
                foreach (var protocol in protocols)
                {
                    foreach (var phase in protocol.Phases)
                    {
                        var temp_template = phase.PlanTemplate;
                        temp_template.Preview = protocol.Preview;
                        temp_templates.Add(temp_template);
                    }
                }
            }

            return GetSiteStatistics<PlanTemplate>(temp_templates.GroupBy(x => x.Preview.TreatmentSite));
        }

        /// <summary>
        /// Gets site statics on objective templates. Treatment site location information.
        /// </summary>
        /// <param name="approvedOnly">Only includes approved templates</param>
        /// <param name="includeClinicalProcotols">If true includes objective templates in clinical protocols.</param>
        /// <returns>List of objective template site statistics</returns>
        public List<SiteStatistics> GetObjectiveTemplateSiteStatistics(bool approvedOnly, bool includeClinicalProtocols)
        {
            var temp_templates = new List<ObjectiveTemplate>(ObjectiveTemplates);
            if (approvedOnly)
            {
                temp_templates = temp_templates.Where(x => x.Preview.ApprovalStatus.Contains("Approved")).ToList();
            }

            if (includeClinicalProtocols)
            {
                var protocols = approvedOnly ? ClinicalProtocols : ClinicalProtocols.Where(x => x.Preview.ApprovalStatus.Contains("Approved"));
                foreach (var protocol in protocols)
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

            return GetSiteStatistics<ObjectiveTemplate>(temp_templates.GroupBy(x => x.Preview.TreatmentSite));
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
                    var normalTissueObjective = optimizationTemplate.Helios.NormalTissueObjective;
                    plan.OptimizationSetup.AddNormalTissueObjective(
                        normalTissueObjective.Priority,
                        normalTissueObjective.DistanceFromTargetBorder,
                        normalTissueObjective.StartDose,
                        normalTissueObjective.EndDose,
                        normalTissueObjective.FallOff);
                }
            }

            List<string> optimizedObjectives = new List<string>();
            List<string> notFoundOptimizationObjectives = new List<string>();
            foreach (var optimizationObjective in optimizationTemplate.ObjectivesAllStructures)
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == optimizationObjective.ID);
                if (structure != null)
                {
                    optimizedObjectives.Add(structure.Id);
                    foreach (var objective in optimizationObjective.StructureObjectives)
                    {
                        if (objective.Type == ObjectiveTypeEnum.Point)
                        {
                            plan.OptimizationSetup.AddPointObjective(
                                structure,
                                objective.Operator == ObjectiveOperatorEnum.Upper
                                    ? OptimizationObjectiveOperator.Upper
                                    : OptimizationObjectiveOperator.Lower,
                                doseUnit == DoseValue.DoseUnit.cGy
                                    ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy)
                                    : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                                objective.Volume.Value,
                                objective.Priority);
                        }
                        else if (objective.Type == ObjectiveTypeEnum.Mean)
                        {
                            plan.OptimizationSetup.AddMeanDoseObjective(structure,
                               doseUnit == DoseValue.DoseUnit.cGy
                                    ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy)
                                    : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                               objective.Priority);
                        }
                        else if (objective.Type == ObjectiveTypeEnum.GEUDs)
                        {
                            plan.OptimizationSetup.AddEUDObjective(
                                structure,
                                objective.Operator == ObjectiveOperatorEnum.Lower
                                    ? OptimizationObjectiveOperator.Lower
                                    : objective.Operator == ObjectiveOperatorEnum.Upper
                                        ? OptimizationObjectiveOperator.Upper
                                        : objective.Operator == ObjectiveOperatorEnum.Target
                                            ? OptimizationObjectiveOperator.Exact
                                            : OptimizationObjectiveOperator.None,
                                doseUnit == DoseValue.DoseUnit.cGy
                                    ? new DoseValue(objective.Dose * 100.0, DoseValue.DoseUnit.cGy)
                                    : new DoseValue(objective.Dose, DoseValue.DoseUnit.Gy),
                                objective.ParameterA.Value,
                                objective.Priority);
                        }
                        else
                        {
                            optimizedObjectives.Add($"ESAPI cannot add line objectives: {objective.Type}");
                        }
                    }
                }
                else
                {
                    notFoundOptimizationObjectives.Add(optimizationObjective.ID);
                }

            }

            if (plan.Beams.FirstOrDefault(x => !x.IsSetupField)?.GantryDirection != GantryDirection.None)
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
                else
                {
                    //optimize with default settings.
                    plan.Optimize();
                }
            }

            return $"Structures Included in Optimization: {string.Join(", ", optimizedObjectives)}{Environment.NewLine}" +
                   $"Structure not found on Structure Set for Optimiztion: {string.Join(", ", notFoundOptimizationObjectives)}";
        }


        /// <summary>
        /// Generate plan from plan template.
        /// Attempts to set the isocenter automatically. If needed, attempts to set as target center
        /// Target determined based on TargetId input, but can be null to use template.
        /// </summary>
        /// <param name="course">(optional) Course where plan should be created (null will generate a new course)</param>
        /// <param name="structureSet">StructureSet where plan should be created</param>
        /// <param name="planTemplate">Plan Template for plan creation</param>
        /// <param name="targetId">(optional) Id of target structure of (null to use template structure)</param>
        /// <returns>The newly generated Plan Setup</returns>
        public ExternalPlanSetup GeneratePlanFromTemplate(Course course, StructureSet structureSet, PlanTemplate planTemplate, string targetId)
        {
            // Generate plan from template. 
            if (course == null)
            {
                course = structureSet.Patient.AddCourse();
                course.Id = GenerateNewCourseId(structureSet.Patient.Courses);
            }

            var currentPlan = course.AddExternalPlanSetup(structureSet);
            int beamCount = 0;
            foreach (var field in planTemplate.Fields.Where(x => !x.Setup))
            {
                // Planning for arc fields
                if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("ARC"))
                {
                    // First check if there is an MLC margin.
                    // If there is an MLC margin, a field needs to be set up so that the MLC margin
                    // can be set throughout the arc trajectory at a sampled control point spacing.
                    // This is done with a conformal arc beam with CP sampling of 20 degrees. 
                    if (field.MLCPlans.FirstOrDefault()?.MLCMargin?.Left != null)
                    {
                        var beam = currentPlan.AddConformalArcBeam(
                            new ExternalBeamMachineParameters(
                                field.TreatmentUnit, 
                                GetEnergyFromBeamTemplate(field.Energy),
                                field.DoseRate, 
                                field.Technique, 
                                field.PrimaryFluenceMode), 
                            field.Collimator.Rtn, 
                            20, 
                            field.Gantry.Rtn, 
                            field.Gantry.StopRtn.Value, 
                            GetGantryDirection(field.Gantry.RtnDirection), 
                            field.TableRtn.Value, 
                            GetIsocenterFromTemplate(field, currentPlan, targetId));
                        beam.Id = field.ID;
                        beam.FitMLCToStructure(new FitToStructureMargins(
                            field.FieldMargin.Left.Value,
                            field.FieldMargin.Bottom ?? field.FieldMargin.Left.Value,
                            field.FieldMargin.Right ?? field.FieldMargin.Left.Value,
                            field.FieldMargin.Top ?? field.FieldMargin.Left.Value), 
                            currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == targetId)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == field.Target.StructureCode.Code)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV"),
                            field.MLCPlans.FirstOrDefault().MLCMargin.OptimizeCollRtnFlag,
                            field.MLCPlans.FirstOrDefault().MLCMargin.JawFittingMode == "0" ? JawFitting.None : JawFitting.FitToRecommended,
                            GetLeafMeetingPoint(field.MLCPlans.FirstOrDefault().ContourMeetPoint),
                            GetClosedLeafMeetingPoint(field.MLCPlans.FirstOrDefault().ClosedMeetPoint));

                        // Check the field fit to see if the jaws are wider than 15cm. 
                        if (beam.ControlPoints.FirstOrDefault().JawPositions.X2 - beam.ControlPoints.FirstOrDefault().JawPositions.X1 > 150.0)
                        {
                            beamCount = FitJawToVMATLimit(beamCount, beam);
                        }
                    }
                    // Margin could be set on the jaw rather than the MLC.
                    else if (field.FieldMargin.Left.HasValue)
                    {
                        var beam = currentPlan.AddConformalArcBeam(
                            new ExternalBeamMachineParameters(
                                field.TreatmentUnit, 
                                GetEnergyFromBeamTemplate(field.Energy), 
                                field.DoseRate, 
                                field.Technique, 
                                field.PrimaryFluenceMode),
                            field.Collimator.Rtn, 
                            20, 
                            field.Gantry.Rtn, 
                            field.Gantry.StopRtn.Value, 
                            GetGantryDirection(field.Gantry.RtnDirection), 
                            field.TableRtn.Value, 
                            GetIsocenterFromTemplate(field, currentPlan, targetId));
                        beam.Id = field.ID;
                        beam.FitCollimatorToStructure(
                            new FitToStructureMargins(field.FieldMargin.Left ?? 0.0,
                                field.FieldMargin.Bottom ?? 0.0, 
                                field.FieldMargin.Right ?? 0.0,
                                field.FieldMargin.Top ?? 0.0),
                            currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == targetId)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == field.Target.StructureCode.Code) 
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV"),
                            field.Collimator.Mode.Contains("X"),
                            field.Collimator.Mode.Contains("Y"),
                            field.FieldMargin.OptimizeCollRtnFlag);

                        if (beam.ControlPoints.FirstOrDefault().JawPositions.X2 - beam.ControlPoints.FirstOrDefault().JawPositions.X1 > 150.0)
                        {
                            beamCount = FitJawToVMATLimit(beamCount, beam);
                        }
                    }
                    else
                    {
                        // If no fitting, a simple arc beam can be used (only 2 control points in arc beam).
                        var beam = currentPlan.AddArcBeam(
                            new ExternalBeamMachineParameters(
                                field.TreatmentUnit,
                                GetEnergyFromBeamTemplate(field.Energy), 
                                field.DoseRate, 
                                field.Technique, 
                                field.PrimaryFluenceMode),
                            new VRect<double>(field.Collimator.X1 * 10.0, field.Collimator.Y1 * 10.0, 
                                              field.Collimator.X2 * 10.0, field.Collimator.Y2 * 10.0), 
                            field.Collimator.Rtn, 
                            field.Gantry.Rtn, 
                            field.Gantry.StopRtn.Value, 
                            GetGantryDirection(field.Gantry.RtnDirection), 
                            field.TableRtn.Value, 
                            GetIsocenterFromTemplate(field, currentPlan, targetId));
                        beam.Id = field.ID;
                    }
                }
                //IMRT settings.
                else if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("IMRT"))
                {
                    // Static beam can be added because IMRT optimization will generate the MLC and sequences. 
                    var b = currentPlan.AddStaticBeam(
                        new ExternalBeamMachineParameters(
                            field.TreatmentUnit, 
                            GetEnergyFromBeamTemplate(field.Energy), 
                            field.DoseRate, 
                            field.Technique, 
                            field.PrimaryFluenceMode),
                        new VRect<double>(field.Collimator.X1 * 10.0, field.Collimator.Y1 * 10.0, 
                                          field.Collimator.X2 * 10.0, field.Collimator.Y2 * 10.0),
                        field.Collimator.Rtn,
                        field.Gantry.Rtn,
                        field.TableRtn.Value,
                        GetIsocenterFromTemplate(field, currentPlan, targetId));
                    b.Id = field.ID;
                    if (field.FieldMargin.BEVMarginFlag)
                    {
                        b.FitCollimatorToStructure(
                            new FitToStructureMargins(
                                field.FieldMargin.Left ?? 0.0, 
                                field.FieldMargin.Bottom ?? 0.0, 
                                field.FieldMargin.Right ?? 0.0, 
                                field.FieldMargin.Top ?? 0.0),
                            currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == targetId)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.Id == field.Target.VolumeID)
                                ?? currentPlan.StructureSet.Structures.FirstOrDefault(x => x.DicomType == "PTV"),
                            field.Collimator.Mode.Contains("X"),
                            field.Collimator.Mode.Contains("Y"),
                            field.FieldMargin.OptimizeCollRtnFlag);
                    }
                }
                else if (planTemplate.Preview.TreatmentStyle.ToUpper().Contains("CONFROMAL"))
                {
                    throw new NotImplementedException("Treatment style 'CONFROMAL' is not supported!");
                }
                else
                {
                    throw new ApplicationException("Could not determine beam type!");
                }
            }

            Debug.WriteLine($"Generate Plan {currentPlan.Id} with the following fields:{Environment.NewLine}Beam Id\tGantry Angle\tTechnique{Environment.NewLine}" +
                            $"{string.Join($"{Environment.NewLine}", currentPlan.Beams.Select(x => new { s = $"{x.Id}\t{x.ControlPoints.First().GantryAngle}\t{x.Technique}" }).Select(x => x.s))}");
            
            return currentPlan;
        }

        /// <summary>
        /// Converts template Prescription class to an Rx for an ESAPI PlanSetup
        /// </summary>
        /// <param name="planTemplate"></param>
        /// <param name="planSetup">Applies the Rx directly to this plan setup.</param>
        /// <param name="doseUnit">Unit of dose (cGy or  Gy)</param>
        public void SetRx(PlanTemplate planTemplate, PlanSetup planSetup, string doseUnit)
        {
            planSetup.SetPrescription(
                planTemplate.FractionCount.Value,
                new DoseValue(
                    doseUnit == "cGy" ? planTemplate.DosePerFraction.Value * 100.0 : planTemplate.DosePerFraction.Value,
                    doseUnit == "cGy" ? DoseValue.DoseUnit.cGy : DoseValue.DoseUnit.Gy),
                planTemplate.PrescribedPercentage.Value);
        }

        public List<DoseMetricModel> CompareProtocolDoseMetrics(PlanningItem plan, ClinicalTemplate template)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper methods for APIs

        /// <summary>
        /// Generate new Course ID (e.g. Auto1)
        /// </summary>
        /// <param name="courses">Existing courses</param>
        /// <returns>New Course ID</returns>
        private string GenerateNewCourseId(IEnumerable<Course> courses)
        {            
            int largestIndex = 0;
            courses.Where(c => c.Id.Contains("Auto")).ToList().ForEach(c =>                
            {
                var number = Regex.Replace(c.Id, "^\\D+", string.Empty);
                var index = int.Parse(number);
                if(largestIndex < index) 
                {
                    largestIndex = index;
                }
            });

            return $"Auto{++largestIndex}";
        }

        /// <summary>
        /// Set Jaw Positions.
        /// Rules:
        /// * Bring the first jaw in 10 cm
        /// * Bring the second jaw in to make 150 mm
        /// </summary>
        /// <param name="beamCount">Count of the beams.</param>
        /// <param name="beam">Beam where the jaw positions has to be set</param>
        /// <returns>Beam with updated jaw positions.</returns>
        private int FitJawToVMATLimit(int beamCount, Beam beam)
        {
            // Bring the first jaw in 10cm
            // Bring the second jaw in to make 150mm.
            var editables = beam.GetEditableParameters();
            double x1 = beam.ControlPoints.First().JawPositions.X1;
            double x2 = beam.ControlPoints.First().JawPositions.X2;
            double fsx = x2 - x1;
            editables.SetJawPositions(
                new VRect<double>(
                    beamCount % 2 == 0 ? x1 + 10.0 : x1 + (fsx - 150.0) - 10.0, 
                    beam.ControlPoints.First().JawPositions.Y1, 
                    beamCount % 2 == 0 ? x2 - (fsx - 150.0) + 10.0 : x2 - 10.0, 
                    beam.ControlPoints.First().JawPositions.Y2));
            beam.ApplyParameters(editables);

            return ++beamCount;
        }

        /// <summary>
        /// Converts template closed leaf meeting point to ESAPI closed leaf meeting point.
        /// </summary>
        /// <param name="closedMeetPoint">Leaf meeting point as defined in the clinical template.</param>
        /// <returns>Closed leaf meeting point</returns>
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

            return ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_Center;
        }

        /// <summary>
        /// Converts open leaf meeting point in template to Eclipse OpenLeavesMeetingPoint enum.
        /// </summary>
        /// <param name="contourMeetingPoint">Cibtiyr neetubgo= oiubt as defined in the clinical template.</param>
        /// <returns>Leaf meeting point</returns>
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

            return OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Middle;
        }

        /// <summary>
        /// Converts templated isocenter to isocenter in ESAPI coordinates. 
        /// </summary>
        /// <param name="beam">Beam where the iscoenter will be placed</param>
        /// <param name="plan">plan with the beam</param>
        /// <param name="targetId">Target ID override. If this is set, the target is assumed to have this id.</param>
        /// <returns>VVector for isocenter position. </returns>
        private VVector GetIsocenterFromTemplate(Field beam, ExternalPlanSetup plan, string targetId)
        {
            double isoCenterX = beam.Isocenter.X;
            double isoCenterY = beam.Isocenter.Y;
            double isoCenterZ = beam.Isocenter.Z;

            // If beam is relative to field target.
            if (beam.Isocenter.Placement == IscoenterPlacementEnum.AFTS || beam.Isocenter.Placement == IscoenterPlacementEnum.RFTS)
            {
                Structure target = null;
                // Check for a structure with the ID TargetId first.
                if (!string.IsNullOrEmpty(targetId))
                {
                    target = plan.StructureSet.Structures.FirstOrDefault(o => o.Id == targetId);
                }

                if (target == null)
                {
                    // Check the template's target volume ID second.
                    target = plan.StructureSet.Structures.FirstOrDefault(o => o.Id == beam.Target.VolumeID);
                    if (target == null)
                    {
                        // Check for any PTV third.
                        target = plan.StructureSet.Structures.FirstOrDefault(o => o.DicomType.Contains("PTV"));
                        if (target == null) 
                        { 
                            throw new ApplicationException("Could not determine target!"); 
                        }
                    }
                }

                // AFTS is at field target center.
                var targetCenterPoint = target.CenterPoint;
                return beam.Isocenter.Placement == IscoenterPlacementEnum.AFTS 
                    ? target.CenterPoint
                    // RFTS is relative to field target center.
                    : new VVector(targetCenterPoint.x + isoCenterX, targetCenterPoint.y + isoCenterY, targetCenterPoint.z + isoCenterZ);
            }
            // At image (user) origin
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.AIO) 
            {
                return plan.StructureSet.Image.UserOrigin;
            }
            // Relative to image (user) origin
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.RIO) 
            {
                var userOrigin = plan.StructureSet.Image.UserOrigin;
                return new VVector(userOrigin.x + isoCenterX, userOrigin.y + isoCenterY, userOrigin.z + isoCenterZ);
            }
            // At image center.
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.AIC)
            {
                return plan.StructureSet.Image.Origin;
            }
            // Relative to image center.
            else if (beam.Isocenter.Placement == IscoenterPlacementEnum.RIC)
            {
                var origin = plan.StructureSet.Image.Origin;
                return new VVector(origin.x + isoCenterX, origin.y + isoCenterY, origin.z + isoCenterZ);
            }

            // The default position will be the user origin.
            return plan.StructureSet.Image.UserOrigin;
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
        /// Deserialize all xml files from the given directory.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="path">Path of the directory.</param>
        /// <returns>List of the deserialized objects.</returns>
        private List<T> DeserializeXmlFile<T>(string path)
        {
            List<T> resultList = new List<T>();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            foreach (var file in Directory.EnumerateFiles(path, "*.xml"))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    var deserialized = ((T)serializer.Deserialize(reader));
                    resultList.Add(deserialized);
                }
            }

            return resultList;
        }

        /// <summary>
        /// Get approves statistic of the given enumeration.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="tempTemplates">Grouped list of T (key, count)</param>
        /// <returns>List of T approves statuses</returns>
        private List<ApprovalStatistics> GetApprovalStatistics<T>(IEnumerable<IGrouping<string, T>> tempTemplates)
        {
            List<ApprovalStatistics> approvals = new List<ApprovalStatistics>();
            foreach (var protocolgroup in tempTemplates)
            {
                approvals.Add(new ApprovalStatistics(protocolgroup.Key, protocolgroup.Count()));
            }

            return approvals;
        }

        /// <summary>
        /// Get site statistic of the given enumeration.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="tempTemplates">Grouped list of T (key, count)</param>
        /// <returns>List of T site statistics</returns>
        private List<SiteStatistics> GetSiteStatistics<T>(IEnumerable<IGrouping<string, T>> tempTemplates)
        {
            List<SiteStatistics> sites = new List<SiteStatistics>();
            foreach (var protocolgroup in tempTemplates)
            {
                sites.Add(new SiteStatistics(protocolgroup.Key, protocolgroup.Count()));
            }

            return sites;
        }

        #endregion
    }
}
