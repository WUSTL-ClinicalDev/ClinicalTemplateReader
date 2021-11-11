using ClinicalTemplateReader.Helpers;
using ClinicalTemplateReader.Models;
using ClinicalTemplateReader.Models.Internals;
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
            ClinicalProtocols = DeserializeXMLFileLogger<Protocol>(Path.Combine(@"\\", imageServer, TEMPLATES_PROTOCOL_PATH));
            ObjectiveTemplates = DeserializeXMLFileLogger<ObjectiveTemplate>(Path.Combine(@"\\", imageServer, TEMPLATES_OBJECTIVE_PATH));
            PlanTemplates = DeserializeXMLFileLogger<PlanTemplate>(Path.Combine(@"\\", imageServer, TEMPLATES_PLAN_PATH));
        }
        private List<T> DeserializeXMLFileLogger<T>(string path)
        {
            List<T> resultList = new List<T>();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            foreach (var file in Directory.EnumerateFiles(path, "*.xml"))
            {
                //Console.WriteLine(file);
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        var deserialized = ((T)serializer.Deserialize(reader));
                        resultList.Add(deserialized);
                    }
                }
                catch (Exception exception)
                {
                    //File.AppendAllText("clinicalTemplateReader_errors.txt", $"Can't deserialize ARIA xml file! File: {file}.{Environment.NewLine} Reason: {exception}");
                    Logger.LogError($"Cannot deserialize ARIA XML file: {file}. {Environment.NewLine} Reason: {exception}");
                }
            }
            return resultList;
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
        /// <param name="machineId">(optional) Override the machine Id</param>
        /// <returns>The newly generated Plan Setup</returns>
        public ExternalPlanSetup GeneratePlanFromTemplate(Course course, StructureSet structureSet, PlanTemplate planTemplate, string targetId, string machineId)
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
                if (field.Technique.ToUpper().Contains("ARC"))
                {
                    // First check if there is an MLC margin.
                    // If there is an MLC margin, a field needs to be set up so that the MLC margin
                    // can be set throughout the arc trajectory at a sampled control point spacing.
                    // This is done with a conformal arc beam with CP sampling of 20 degrees. 
                    if (field.MLCPlans.FirstOrDefault()?.MLCMargin?.Left != null)
                    {
                        var beam = currentPlan.AddConformalArcBeam(
                            new ExternalBeamMachineParameters(
                                String.IsNullOrEmpty(machineId) ? field.TreatmentUnit : machineId,
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
                                String.IsNullOrEmpty(machineId) ? field.TreatmentUnit : machineId,
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
                                String.IsNullOrEmpty(machineId) ? field.TreatmentUnit : machineId,
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
                            String.IsNullOrEmpty(machineId) ? field.TreatmentUnit : machineId,
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

        public List<DoseMetricModel> CompareProtocolDoseMetrics(PlanningItem plan, Protocol template)
        {
            List<DoseMetricModel> doseMetricModel = new List<DoseMetricModel>();
            if (template.Phases == null || template.Phases.Count() == 0 || template.Phases.All(x => x.Prescription == null))
            {
                return doseMetricModel;
            }
            foreach (var prescription in template.Phases.Where(x => x.Prescription != null).Select(x => x.Prescription))
            {
                if (prescription.Items != null)
                {
                    foreach (var item in prescription.Items)
                    {
                        doseMetricModel.Add(FormatItemToDoseMetric(item, plan));
                    }
                }
                if (prescription.MeasureItem != null)
                {
                    foreach (var measureItem in prescription.MeasureItem)
                    {
                        doseMetricModel.Add(FormatMeasureItemToDoseMetric(measureItem, plan));
                    }
                }
            }
            return doseMetricModel;

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
                if (largestIndex < index)
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
                bool afts = beam.Isocenter.Placement == IscoenterPlacementEnum.AFTS;
                var beamIsoX = afts ? target.CenterPoint.x : target.CenterPoint.x + isoCenterX;
                var beamIsoY = afts ? target.CenterPoint.y : target.CenterPoint.y + isoCenterY;
                var beamIsoZ = afts ? target.CenterPoint.z : target.CenterPoint.z + isoCenterZ;
                //round to nearest mm for shift from userorigin.
                var uoX = plan.StructureSet.Image.UserOrigin.x;
                var uoY = plan.StructureSet.Image.UserOrigin.y;
                var uoZ = plan.StructureSet.Image.UserOrigin.z;
                var remainderX = (beamIsoX - uoX) - Math.Round((beamIsoX - uoX), 0);
                var remainderY = (beamIsoY - uoY) - Math.Round((beamIsoY - uoY), 0);
                var remainderZ = (beamIsoZ - uoZ) - Math.Round((beamIsoZ - uoZ), 0);
                var isoX = beamIsoX - remainderX;
                var isoY = beamIsoY - remainderY;
                var isoZ = beamIsoZ - remainderZ;

                return new VVector(isoX, isoY, isoZ);
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
        /// <summary>
        /// Converts Measure Item from Clinical Protocols into Internal DoseMetricModel
        /// </summary>
        /// <param name="measureItem">Measure item from Clinical Protocol</param>
        /// <param name="plan">Planning Item to be compared</param>
        /// <returns>DoseMetricModel</returns>
        private DoseMetricModel FormatMeasureItemToDoseMetric(MeasureItem measureItem, PlanningItem plan)
        {
            var doseMetric = new DoseMetricModel();
            //int numFx = GetNumberOfFractions(plan);
            doseMetric.StructureId = measureItem.ID;
            if (measureItem.Type == TypeEnum.DoseAtAbsoluteVolume || measureItem.Type == TypeEnum.DoseAtRelativeVolume)
            {
                doseMetric.MetricType = DoseMetricTypeEnum.DoseAtVolume;
                if (!plan.StructureSet.Structures.Any(x => x.Id == measureItem.ID))
                {
                    doseMetric.ResultText = "Structure Not Found";
                    doseMetric.Pass = PassResultEnum.NA;

                    return doseMetric;
                }
                else if (plan.StructureSet.Structures.FirstOrDefault(x => x.Id == measureItem.ID).IsEmpty)
                {
                    doseMetric.ResultText = "Empty";
                    doseMetric.Pass = PassResultEnum.NA;
                    return doseMetric;
                }
                doseMetric.InputUnit = measureItem.Type == TypeEnum.DoseAtAbsoluteVolume ? ResultUnitEnum.cc : ResultUnitEnum.PercentVolume;
                doseMetric.InputValue = (double)measureItem.TypeSpecifier;
                //the value has been null in some instances
                doseMetric.TargetValue = measureItem.Value != null ? (double)measureItem.Value : 0.0;
                doseMetric.TargetUnit = measureItem.ReportDQPValueInAbsoluteUnits ? ResultUnitEnum.Gy : ResultUnitEnum.PercentDose;
                var planDoseUnit = GetPlanDoseUnit(plan);
                //convert target unit to system unit.
                if (doseMetric.TargetUnit != ResultUnitEnum.PercentDose && doseMetric.TargetUnit.ToString() != planDoseUnit.ToString())
                {
                    doseMetric.TargetUnit = ResultUnitEnum.cGy;
                    doseMetric.TargetValue = (double)measureItem.Value * 100.0;
                }
                doseMetric.ResultUnit = doseMetric.TargetUnit;
                //Get result from DVH. 
                double resultValue = GetDoseResultFromItem(doseMetric, plan);
                doseMetric.ResultValue = resultValue;
                //Compare to target expectation.
                switch (measureItem.Modifier)
                {
                    case MeasureItemModifierEnum.Is:
                        doseMetric.MetricText = $"{doseMetric.StructureId} D{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] equals {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue == doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                    case MeasureItemModifierEnum.IsLessThan:
                        doseMetric.MetricText = $"{doseMetric.StructureId} D{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] is less than {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue < doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                    case MeasureItemModifierEnum.IsMoreThan:
                        doseMetric.MetricText = $"{doseMetric.StructureId} D{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] is more than {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue > doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                }
                doseMetric.ResultText = $"{doseMetric.StructureId} D{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] = {doseMetric.ResultValue:F2} {ConvertUnitToString(doseMetric.ResultUnit)}";

            }
            else if (measureItem.Type == TypeEnum.VolumeAtAbsoluteDose || measureItem.Type == TypeEnum.VolumeAtRelativeDose)
            {
                doseMetric.MetricType = DoseMetricTypeEnum.VolumeAtDose;
                if (!plan.StructureSet.Structures.Any(x => x.Id == measureItem.ID))
                {
                    doseMetric.ResultText = "Structure Not Found";
                    doseMetric.Pass = PassResultEnum.NA;
                    return doseMetric;
                }
                doseMetric.InputUnit = measureItem.Type == TypeEnum.VolumeAtAbsoluteDose ? ResultUnitEnum.Gy : ResultUnitEnum.PercentDose;
                doseMetric.InputValue = (double)measureItem.TypeSpecifier;
                doseMetric.TargetUnit = measureItem.ReportDQPValueInAbsoluteUnits ? ResultUnitEnum.cc : ResultUnitEnum.PercentVolume;
                doseMetric.TargetValue = measureItem.Value != null ? (double)measureItem.Value : 0.0;
                var planDoseUnit = GetPlanDoseUnit(plan);
                //convert target unit to system unit.
                if (doseMetric.InputUnit != ResultUnitEnum.PercentDose && doseMetric.InputUnit.ToString() != planDoseUnit.ToString())
                {
                    doseMetric.InputUnit = ResultUnitEnum.cGy;
                    doseMetric.InputValue = (double)measureItem.TypeSpecifier * 100.0;
                }
                doseMetric.ResultUnit = doseMetric.TargetUnit;
                double resultValue = GetVolumeResultFromItem(doseMetric, plan);
                doseMetric.ResultValue = resultValue;
                switch (measureItem.Modifier)
                {
                    case MeasureItemModifierEnum.Is:
                        doseMetric.MetricText = $"{doseMetric.StructureId} V{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] equals {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue == doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                    case MeasureItemModifierEnum.IsLessThan:
                        doseMetric.MetricText = $"{doseMetric.StructureId} V{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] is less than {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue < doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                    case MeasureItemModifierEnum.IsMoreThan:
                        doseMetric.MetricText = $"{doseMetric.StructureId} V{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] is more than {doseMetric.TargetValue} {ConvertUnitToString(doseMetric.TargetUnit)}";
                        doseMetric.Pass = measureItem.Value == null ? PassResultEnum.NA : doseMetric.ResultValue > doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                        break;
                }
                doseMetric.ResultText = $"{doseMetric.StructureId} V{doseMetric.InputValue}{ConvertUnitToString(doseMetric.InputUnit)}[{ConvertUnitToString(doseMetric.ResultUnit)}] = {doseMetric.ResultValue:F2} {ConvertUnitToString(doseMetric.ResultUnit)}";

            }
            else
            {
                doseMetric.ResultText = "Conformity Index and Gradient Measure not supported in this version.";
                doseMetric.Pass = PassResultEnum.NA;
            }
            return doseMetric;

        }
        /// <summary>
        /// Get DoseMetricModel from clinical protocol item.
        /// </summary>
        /// <param name="item">Protocol prescription item</param>
        /// <param name="plan">Planning item (plan or plansum)</param>
        /// <returns>DoseMetricModel</returns>
        private DoseMetricModel FormatItemToDoseMetric(Item item, PlanningItem plan)
        {
            var doseMetric = new DoseMetricModel();
            //int numFx = GetNumberOfFractions(plan);
            doseMetric.StructureId = item.ID;
            doseMetric.TargetUnit = ResultUnitEnum.Gy;//items are always in Gy.
            doseMetric.TargetValue = (double)item.TotalDose;
            //convert target unit to system unit.
            var doseUnit = GetPlanDoseUnit(plan);
            if (doseUnit.ToString() != doseMetric.TargetUnit.ToString())
            {
                doseMetric.TargetUnit = ResultUnitEnum.cGy;
                doseMetric.TargetValue = ((double)item.TotalDose) * 100.0;
            }
            if (!plan.StructureSet.Structures.Any(x => x.Id == item.ID))
            {
                doseMetric.ResultText = "Structure Not Found";
                doseMetric.Pass = PassResultEnum.NA;
                return doseMetric;
            }
            else if (plan.StructureSet.Structures.FirstOrDefault(x => x.Id == item.ID).IsEmpty)
            {
                doseMetric.ResultText = "Empty";
                doseMetric.Pass = PassResultEnum.NA;
                return doseMetric;
            }
            else
            {
                if (item.Modifier == ModifierEnum.AtLeast || item.Modifier == ModifierEnum.AtMost)
                {
                    doseMetric.InputValue = item.Parameter;
                    doseMetric.InputUnit = ResultUnitEnum.PercentVolume;
                    doseMetric.MetricType = DoseMetricTypeEnum.DoseAtVolume;
                    doseMetric.MetricText = $"{doseMetric.StructureId} at least {doseMetric.InputValue} % recieves more than {doseMetric.TargetValue}{doseMetric.TargetUnit}";
                    doseMetric.ResultUnit = doseMetric.TargetUnit;
                    double resultValue = GetDoseResultFromItem(doseMetric, plan);
                    doseMetric.ResultValue = resultValue;
                    if (item.Modifier == ModifierEnum.AtMost)
                    {
                        //the pass criteria for this metric ("AtMost") is different than what Eclipse will show. I believe Eclipse may pass this metric in error.
                        doseMetric.Pass = doseMetric.ResultValue <= doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                    }
                    else
                    {
                        doseMetric.Pass = doseMetric.ResultValue >= doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                    }
                    doseMetric.ResultText = $"{doseMetric.StructureId} {doseMetric.InputValue} % receives {doseMetric.ResultValue:F2}{doseMetric.ResultUnit}";
                }
                else if (item.Modifier == ModifierEnum.MaxDoseIs || item.Modifier == ModifierEnum.MaxDoseIsLessThan || item.Modifier == ModifierEnum.MeanDoseIs ||
                    item.Modifier == ModifierEnum.MeanDoseIsLessThan || item.Modifier == ModifierEnum.MeanDoseIsMoreThan || item.Modifier == ModifierEnum.MinDoseIs ||
                    item.Modifier == ModifierEnum.MinDoseIsMoreThan)
                {
                    switch (item.Modifier)
                    {
                        case ModifierEnum.MaxDoseIs:
                        case ModifierEnum.MaxDoseIsLessThan:
                            doseMetric.MetricType = DoseMetricTypeEnum.MaxDose;
                            doseMetric.MetricText = $"{doseMetric.StructureId} Max Dose is {(item.Modifier == ModifierEnum.MaxDoseIsLessThan ? "less than " : "")}{doseMetric.TargetValue}{doseMetric.TargetUnit}";
                            break;
                        case ModifierEnum.MeanDoseIs:
                        case ModifierEnum.MeanDoseIsLessThan:
                        case ModifierEnum.MeanDoseIsMoreThan:
                            doseMetric.MetricType = DoseMetricTypeEnum.MeanDose;
                            doseMetric.MetricText = $"{doseMetric.StructureId} Mean Dose is {(item.Modifier == ModifierEnum.MeanDoseIs ? "" : item.Modifier == ModifierEnum.MeanDoseIsLessThan ? "less than " : "more than ")}{doseMetric.TargetValue}{doseMetric.TargetUnit}";
                            break;
                        case ModifierEnum.MinDoseIs:
                        case ModifierEnum.MinDoseIsMoreThan:
                            doseMetric.MetricType = DoseMetricTypeEnum.MinDose;
                            doseMetric.MetricText = $"{doseMetric.StructureId} Min Dose is {(item.Modifier == ModifierEnum.MinDoseIsMoreThan ? "more than " : "")}{doseMetric.TargetValue}{doseMetric.TargetUnit}";
                            break;
                    }
                    doseMetric.ResultUnit = doseMetric.TargetUnit;
                    double resultValue = GetDoseResultFromItem(doseMetric, plan);
                    doseMetric.ResultValue = resultValue;
                    //get passing for metric.
                    switch (item.Modifier)
                    {
                        case ModifierEnum.MaxDoseIs:
                        case ModifierEnum.MeanDoseIs:
                        case ModifierEnum.MinDoseIs:
                            doseMetric.Pass = doseMetric.ResultValue == doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                            break;
                        case ModifierEnum.MeanDoseIsLessThan:
                        case ModifierEnum.MaxDoseIsLessThan:
                            doseMetric.Pass = doseMetric.ResultValue < doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                            break;
                        case ModifierEnum.MeanDoseIsMoreThan:
                        case ModifierEnum.MinDoseIsMoreThan:
                            doseMetric.Pass = doseMetric.ResultValue > doseMetric.TargetValue ? PassResultEnum.Pass : PassResultEnum.Fail;
                            break;
                    }
                    doseMetric.ResultText = $"{doseMetric.StructureId} {doseMetric.MetricType} = {doseMetric.ResultValue:F2}{doseMetric.ResultUnit}";
                }
                else
                {
                    doseMetric.ResultText = "Reference Points Isodose lines and depths not supported in this version";
                    doseMetric.Pass = PassResultEnum.NA;
                }
                return doseMetric;
            }
        }
        /// <summary>
        /// Converts the Unit enum to string for metric text
        /// </summary>
        /// <param name="inputUnit">ResultUnitEnum unit</param>
        /// <returns>string representing the unit. The only difference from ToString() is PercentVolume and PercentDose are converted to '%'</returns>
        private string ConvertUnitToString(ResultUnitEnum inputUnit)
        {
            switch (inputUnit)
            {
                case ResultUnitEnum.PercentDose:
                case ResultUnitEnum.PercentVolume:
                    return "%";
                case ResultUnitEnum.cGy:
                    return "cGy";
                case ResultUnitEnum.cc:
                    return "cc";
                case ResultUnitEnum.Gy:
                    return "Gy";
                default:
                    return String.Empty;
            }
        }
        /// <summary>
        /// Extracts DVH Dose value corresponding to metric
        /// </summary>
        /// <param name="doseMetric">Currently built dose metric.</param>
        /// <param name="plan">Planning Item from which the DVH should be extracted.</param>
        /// <returns>double, dose value in unit of return value.</returns>
        private double GetDoseResultFromItem(DoseMetricModel doseMetric, PlanningItem plan)
        {
            Structure structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == doseMetric.StructureId);
            var doseUnit = GetPlanDoseUnit(plan);
            var dvh = plan.GetDVHCumulativeData(structure,
                doseMetric.ResultUnit == ResultUnitEnum.PercentDose ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                doseMetric.InputUnit == ResultUnitEnum.cc ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative,
                doseUnit == DoseValue.DoseUnit.Gy ? 0.01 : 0.1);
            double returnDose = 0.0;
            switch (doseMetric.MetricType)
            {
                case DoseMetricTypeEnum.MaxDose:
                    returnDose = dvh.MaxDose.Dose;
                    break;
                case DoseMetricTypeEnum.MinDose:
                    returnDose = dvh.MinDose.Dose;
                    break;
                case DoseMetricTypeEnum.MeanDose:
                    returnDose = dvh.MeanDose.Dose;
                    break;
                default:
                    if (doseMetric.InputValue == 100 && doseMetric.InputUnit == ResultUnitEnum.PercentVolume)
                    {
                        returnDose = dvh.MinDose.Dose;
                    }
                    else if (doseMetric.InputValue < dvh.CurveData.Min(x => x.Volume))
                    {
                        returnDose = dvh.MaxDose.Dose;
                    }
                    else
                    {
                        returnDose = dvh.CurveData.FirstOrDefault(x => x.Volume <= doseMetric.InputValue).DoseValue.Dose;
                    }
                    break;
            }
            //convert returned dose if necessary.
            if (doseMetric.ResultUnit != ResultUnitEnum.PercentDose && doseMetric.ResultUnit.ToString() != doseUnit.ToString())
            {
                //dose unit in Gy but result should be cGy
                if (doseMetric.ResultUnit == ResultUnitEnum.Gy)
                {
                    returnDose = returnDose * 100.0;
                }
                else//dose unit in cGy but result should be Gy
                {
                    returnDose = returnDose / 100.0;
                }
            }
            return returnDose;
        }
        /// <summary>
        /// Extract DVH Volume from corresponding dose metric
        /// </summary>
        /// <param name="doseMetric">Currently being built dose metric</param>
        /// <param name="plan">Planning Item for DVH extraction</param>
        /// <returns>Volume value from DVH metric in return unit.</returns>
        private double GetVolumeResultFromItem(DoseMetricModel doseMetric, PlanningItem plan)
        {
            Structure structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == doseMetric.StructureId);
            var doseUnit = GetPlanDoseUnit(plan);
            var dvh = plan.GetDVHCumulativeData(structure,
                doseMetric.InputUnit == ResultUnitEnum.PercentDose ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                doseMetric.ResultUnit == ResultUnitEnum.cc ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative,
                doseUnit == DoseValue.DoseUnit.Gy ? 0.01 : 0.1);
            var doseInput = doseMetric.InputValue;
            //convert input dose unit prior to comparison.
            if (doseMetric.InputUnit != ResultUnitEnum.PercentDose && doseMetric.InputUnit.ToString() != doseUnit.ToString())
            {
                if (doseMetric.InputUnit == ResultUnitEnum.Gy)
                {
                    doseInput = doseInput * 100.0;
                }
                else
                {
                    doseInput = doseInput / 100.0;
                }
            }
            double returnVolume = dvh.CurveData.Max(x => x.DoseValue.Dose) < doseMetric.InputValue ? 0.0 : dvh.CurveData.FirstOrDefault(x => x.DoseValue.Dose >= doseInput).Volume;
            return returnVolume;
        }
        /// <summary>
        /// Get Dose Unit from Treatment plan
        /// </summary>
        /// <param name="plan">Planning Item to be converted to plansetup or plansum.</param>
        /// <returns>Dose Unit from plan's TotalDose</returns>
        internal static DoseValue.DoseUnit GetPlanDoseUnit(PlanningItem plan)
        {
            if (plan is PlanSum)
            {
                return (plan as PlanSum).PlanSetups.FirstOrDefault().TotalDose.Unit;
            }
            else if (plan is PlanSetup)
            {
                return (plan as PlanSetup).TotalDose.Unit;
            }
            else { return DoseValue.DoseUnit.Unknown; }

        }
        #endregion
    }
}
