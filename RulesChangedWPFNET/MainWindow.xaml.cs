using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RulesChangedWPFNET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    // A class with static members only
    // used for global-accessable properties

    public class GlobalProperty
    {
        public static bool FileOpened = false;
        public static bool FileChanged = false;

        public enum SublistIndex
        {
            Uncategorized,
            Countries,
            InfantryTypes,
            VehicleTypes,
            AircraftTypes,
            BuildingTypes,
            TerrainTypes,
            SmudgeTypes,
            OverlayTypes,
            Animations,
            VoxelAnims,
            Particles,
            ParticleSystems,
            SuperWeaponTypes,
            Warheads,
            AIGenerals,
            VariableNames,
            MAX
        }
    }

    public partial class MainWindow : Window
    {
        private static readonly Regex sWhitespace = new Regex(@"\s\s+|\t+"); // saves for future use

        string exportPath = ".\\input_output\\results";
        string rulesFilePath = ".\\input_output\\rules.ini";
        int fieldsCount = 18; // The count and the ruleFilepath is fixed for rules.ini; currently we don't test on rulesmd.ini yet

        public Dictionary<string, GlobalProperty.SublistIndex> tagCategorizedList = new Dictionary<string, GlobalProperty.SublistIndex>();
        Dictionary<string, GlobalProperty.SublistIndex> predefinedTag = new Dictionary<string, GlobalProperty.SublistIndex>();
        public List<Dictionary<string, Hashtable>> dataSets = new List<Dictionary<string, Hashtable>>();

        public MainWindow()
        {
            InitializeComponent();
            Building_button.IsEnabled = false;
            Menu_file_save.IsEnabled = false;
            GlobalProperty.FileChanged = false;
            GlobalProperty.FileOpened = false;
            initialDataPrepare(); // may need to change to file_open() for rulesmd.ini
        }

        private void MenuItem_Click_file_open(object sender, RoutedEventArgs e)
        {
            if (GlobalProperty.FileOpened == true)
            {
                tagCategorizedList.Clear();
            }

            if (readFromRules() == true) // file location is hardcoded, will change later.
            {
                Building_button.IsEnabled = true;
                Menu_file_save.IsEnabled = true;
            }
            else
            {
                Building_button.IsEnabled = false;
                Menu_file_save.IsEnabled = false;
            }
        }

        private bool readFromRules()
        {
            Regex sWhitespace = new Regex(@"\s\s+|\t+");
            int semiIndex = 0;
            string currentTag = "space_holder"; // it does not have any usage
            string processedLine;
            bool hashTableCreateIndication = false;
            GlobalProperty.SublistIndex currentSublistIndex = GlobalProperty.SublistIndex.Uncategorized;
            initialDataSets(fieldsCount);

            foreach (string line in File.ReadLines(rulesFilePath))
            {
                if (line.Length == 0)
                    continue;

                if (line[0] == ';')
                    continue;

                if ((semiIndex = line.IndexOf(';')) >= 0)
                {
                    processedLine = sWhitespace.Replace(line.Substring(0, semiIndex), "");
                }
                else
                {
                    processedLine = sWhitespace.Replace(line, "");
                }

                if (processedLine == "")
                {
                    continue;
                }

                if (processedLine[0] == '[') // A tag
                {
                    currentTag = processedLine.Split('[', ']')[1]; // cut the bracket, extract the tag
                    if (predefinedTag.ContainsKey(currentTag))
                    {
                        // pre-defined?  if it is pre-defined, then we change currentTag and sublistindex.
                        currentSublistIndex = predefinedTag[currentTag];
                        hashTableCreateIndication = true; // we are adding something to dataSets[index] in later reading.
                        continue;
                    }
                    else
                    {
                        hashTableCreateIndication = false; // don't do that since we do not create entry<string, hashTable> here.
                        // The tag is not pre-defined, check if it is registered
                        if (tagCategorizedList.ContainsKey(currentTag))
                        {
                            // change index, continue next line
                            currentSublistIndex = tagCategorizedList[currentTag];
                            continue;
                        }
                        else
                        {
                            /* There is a potential bug. When code runs into here, I assume the code has finished all registration
                             * of [BuildingTypes] and etc., however it might be incorrect since rules.ini do not force such arrangement,
                             * i.e. you could put [BuildingTypes] after [NAPOWR] without issue.
                             */
                            try
                            {
                                /* register to tagCategorizedList; since we made above assumption, the new tag
                                 * is: 1. not predefined; 2. not registered. It will make into uncategorized tag.
                                 * However, we may need some change to process weapons and warheads since they
                                 * do not need to "register" in rule files.
                                */
                                currentSublistIndex = GlobalProperty.SublistIndex.Uncategorized;
                                tagCategorizedList.Add(currentTag, currentSublistIndex);
                                dataSets[(int)currentSublistIndex].Add(currentTag, new Hashtable());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                Console.WriteLine(processedLine);
                            }
                        }
                    }
                }
                else
                {
                    // the line is not new tag, write to attributes according to currentTag and currentSublistIndex
                    if (hashTableCreateIndication == true)
                    {
                        try
                        {
                            // we are reading "registration list" here, means we are creating key-value pair for the items of list
                            string[] fields = processedLine.Split("=");
                            tagCategorizedList.Add(fields[fields.Length - 1], currentSublistIndex);
                            dataSets[(int)currentSublistIndex].Add(fields[fields.Length - 1], new Hashtable());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(processedLine);
                        }

                    }
                    else
                    {
                        // we are reading attributes table here
                        string[] fields = processedLine.Split("=");
                        dataSets[(int)currentSublistIndex][currentTag][fields[0]] = fields[fields.Length - 1];
                    }
                }

            }

            return true;
        }

        private void initialDataPrepare()
        {
            predefinedTag.Add("Countries", GlobalProperty.SublistIndex.Countries);
            predefinedTag.Add("InfantryTypes", GlobalProperty.SublistIndex.InfantryTypes);
            predefinedTag.Add("VehicleTypes", GlobalProperty.SublistIndex.VehicleTypes);
            predefinedTag.Add("AircraftTypes", GlobalProperty.SublistIndex.AircraftTypes);
            predefinedTag.Add("BuildingTypes", GlobalProperty.SublistIndex.BuildingTypes);
            predefinedTag.Add("TerrainTypes", GlobalProperty.SublistIndex.TerrainTypes);
            predefinedTag.Add("SmudgeTypes", GlobalProperty.SublistIndex.SmudgeTypes);
            predefinedTag.Add("OverlayTypes", GlobalProperty.SublistIndex.OverlayTypes);
            predefinedTag.Add("Animations", GlobalProperty.SublistIndex.Animations);
            predefinedTag.Add("VoxelAnims", GlobalProperty.SublistIndex.VoxelAnims);
            predefinedTag.Add("Particles", GlobalProperty.SublistIndex.Particles);
            predefinedTag.Add("ParticleSystems", GlobalProperty.SublistIndex.ParticleSystems);
            predefinedTag.Add("SuperWeaponTypes", GlobalProperty.SublistIndex.SuperWeaponTypes);
            predefinedTag.Add("Warheads", GlobalProperty.SublistIndex.Warheads);
            predefinedTag.Add("AIGenerals", GlobalProperty.SublistIndex.AIGenerals);
            predefinedTag.Add("VariableNames", GlobalProperty.SublistIndex.VariableNames);
        }

        private void initialDataSets(int fc)
        {
            dataSets.Clear(); // will here, i.e. clear a List of Dictionary<string, Hashtable> cause memory leak? need to investigate
            for (int i = 0; i < fc; i++)
            {
                dataSets.Add(new Dictionary<string, Hashtable>());
            }
        }

        private void MenuItem_Click_file_save(object sender, RoutedEventArgs e)
        {
            /* Write the contents back to the file.
             * However, for debugging purpose, we will write to a separate file.
             */

            // flush the file in exportPath
            File.Delete(exportPath);

            // write first tag, [BuildingTypes]
            StreamWriter sw = new StreamWriter(exportPath, true, Encoding.ASCII);
            sw.Close();

        }

        private void Building_Button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.Uncategorized);
        }

        private void Open_New_Window(GlobalProperty.SublistIndex categoryIndex)
        {
            CategoryUnitListWindow newWindow = new CategoryUnitListWindow(dataSets[(int)categoryIndex], this);
            newWindow.Show();
            this.Hide();
        }

    }
}
