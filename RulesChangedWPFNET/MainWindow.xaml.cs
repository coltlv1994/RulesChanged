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
            InfantryTypes, //     0,
            VehicleTypes, //      1
            AircraftTypes, //     2
            BuildingTypes, //     3
            Warheads, //          4
            DummyTags,  //        5
            Projectiles, //       6
            Weapons, //           7
            Uncategorized, //     8, it must be at the end
            MAX //                9
        }
    }

    public partial class MainWindow : Window
    {
        private static readonly Regex sWhitespace = new Regex(@"\s\s+|\t+"); // saves for future use

        string rulesFilePath = "";
        string uncategorizedTagExportPath = ".\\output\\uncategorized_tags";
        int fieldsCount = ((int)GlobalProperty.SublistIndex.MAX); //Dummy tags do not need any field.

        public Dictionary<string, GlobalProperty.SublistIndex> tagCategorizedList = new();
        Dictionary<string, GlobalProperty.SublistIndex> predefinedTag = new Dictionary<string, GlobalProperty.SublistIndex>();
        public List<Dictionary<string, Hashtable>> dataSets = new List<Dictionary<string, Hashtable>>();
        public List<string> dummyLinesToWrite = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Menu_file_save.IsEnabled = false;
            GlobalProperty.FileChanged = false;
            GlobalProperty.FileOpened = false;
            disable_buttons();
            initialDataPrepare(); // may need to change to file_open() for rulesmd.ini
        }

        private void MenuItem_Click_file_open(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "RA2 rule file|rules.ini|RA2:YR rule file|rulesmd.ini";
            dlg.Multiselect = false;
            Nullable<bool> openFileDialogResult = dlg.ShowDialog();

            if (openFileDialogResult == true)
            {
                // flush previously opened file
                tagCategorizedList.Clear();
                dataSets.Clear();
                dummyLinesToWrite.Clear();
                // As per instructions on the forum, some hard-coded fields must be treated as "dummy"
                tagCategorizedList.Add("Colors", GlobalProperty.SublistIndex.DummyTags);
                tagCategorizedList.Add("Sides", GlobalProperty.SublistIndex.DummyTags);
                tagCategorizedList.Add("ColorAdd", GlobalProperty.SublistIndex.DummyTags);

                // file select
                rulesFilePath = dlg.FileName;
                if (readFromRules() == true)
                {
                    enable_buttons();
                    Menu_file_save.IsEnabled = true;
                    GlobalProperty.FileOpened = true;
                }
                else
                {
                    // file read fail
                    ; // nothing happens; maybe should pop a MessageBox.
                }
            }
            else
            {
                // file not select
                // nothing happens
                ;
            }
        }

        private bool readFromRules()
        {
            Regex sWhitespace2 = new Regex(@"\s\s+|\t+");
            Regex sWhitespace1 = new Regex(@"\s+");
            int semiIndex = 0;
            string currentTag = "space_holder"; // it does not have any usage
            string processedLine;
            bool hashtableCreateIndication = false;
            GlobalProperty.SublistIndex currentSublistIndex = GlobalProperty.SublistIndex.Uncategorized;
            initialDataSets(fieldsCount);

            foreach (string line in File.ReadLines(rulesFilePath))
            {
                if (line.Length == 0)
                {
                    dummyLinesToWrite.Add(line);
                    continue;
                }

                if (line[0] == ';')
                {
                    continue;
                }

                if ((semiIndex = line.IndexOf(';')) >= 0)
                {
                    processedLine = sWhitespace2.Replace(line.Substring(0, semiIndex), "");
                }
                else
                {
                    processedLine = sWhitespace2.Replace(line, "");
                }

                if (processedLine == "")
                {
                    continue;
                }

                if (processedLine[0] == '[') // A tag
                {
                    tagDetectedAndTreat(processedLine,
                                        ref hashtableCreateIndication, ref currentTag,
                                        ref currentSublistIndex);
                }
                else
                {
                    /* This line is not a tag.
                     * Two fields works here, hashtableCreationIndication and dummyTag.
                     * HCI==yes and dummyTag==yes, register the tag but don't create hashtable, write to List<>;
                     * HCI==yes and dummyTag==no, register and create, don't write to List<>;
                     * HCI==no and dummyTag==yes, don't register and don't create, write to List<>;
                     * HCI==no and dummytag==no, register its value and key to dataSets;
                     */
                    // the line is not new tag, write to attributes according to currentTag and currentSublistIndex
                    if (hashtableCreateIndication == true)
                    {
                        try
                        {
                            // we are reading "registration list" here
                            string[] fields = processedLine.Split("=");
                            string fieldWithoutWhitespace = sWhitespace1.Replace(fields[fields.Length - 1], "");
                            tagCategorizedList.Add(fieldWithoutWhitespace, currentSublistIndex);
                            if (currentSublistIndex != GlobalProperty.SublistIndex.DummyTags)
                            {
                                // dummy tags do not require any data fields
                                dataSets[(int)currentSublistIndex].Add(fieldWithoutWhitespace, new Hashtable());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine(processedLine);
                        }

                    }
                    else
                    {
                        if (currentSublistIndex != GlobalProperty.SublistIndex.DummyTags)
                        {
                            // we are reading attributes table here, dummy tags do not need read anything
                            string[] fields = processedLine.Split("=");
                            if (fields[0] == "Name")
                            {
                                dataSets[(int)currentSublistIndex][currentTag][fields[0]] = fields[fields.Length - 1];
                            }
                            else
                            {
                                dataSets[(int)currentSublistIndex][currentTag][fields[0]] = sWhitespace1.Replace(fields[fields.Length - 1], "");
                            }
                            
                        }
                    }
                }

                if (currentSublistIndex == GlobalProperty.SublistIndex.DummyTags)
                {
                    dummyLinesToWrite.Add(processedLine);
                }

            }

            // Post process, for weapons and projectiles.
            postprocessUncategorized();

            return true;
        }

        private void initialDataPrepare()
        {
            predefinedTag.Add("Countries", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("InfantryTypes", GlobalProperty.SublistIndex.InfantryTypes);
            predefinedTag.Add("VehicleTypes", GlobalProperty.SublistIndex.VehicleTypes);
            predefinedTag.Add("AircraftTypes", GlobalProperty.SublistIndex.AircraftTypes);
            predefinedTag.Add("BuildingTypes", GlobalProperty.SublistIndex.BuildingTypes);
            predefinedTag.Add("TerrainTypes", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("SmudgeTypes", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("OverlayTypes", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("Animations", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("VoxelAnims", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("Particles", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("ParticleSystems", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("SuperWeaponTypes", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("Warheads", GlobalProperty.SublistIndex.Warheads);
            predefinedTag.Add("AIGenerals", GlobalProperty.SublistIndex.DummyTags);
            predefinedTag.Add("VariableNames", GlobalProperty.SublistIndex.DummyTags);
        }

        private void initialDataSets(int fc)
        {
            dataSets.Clear(); // will here, i.e. clear a List of Dictionary<string, Hashtable> cause memory leak? need to investigate
            for (int i = 0; i < fc; i++)
            {
                dataSets.Add(new Dictionary<string, Hashtable>());
            }
        }

        // explicitly require the boolean values pass-by-reference
        private bool tagDetectedAndTreat(string inputLine, ref bool hashtableCreationMode,
                                         ref string newTag, ref GlobalProperty.SublistIndex newTagIndex)
        {
            newTag = inputLine.Split('[', ']')[1]; // cut the bracket, extract the tag
            if (predefinedTag.ContainsKey(newTag))
            {
                // If it is pre-defined, which means we hit to a list that requires to be registered.
                // However, some dummy tags in certain list require registration otherwise uncategorized list is too large and unwanted.
                newTagIndex = predefinedTag[newTag];
                hashtableCreationMode = true; // we are adding something to dataSets[index] in later reading.
                return true;
            }
            else
            {
                hashtableCreationMode = false; // don't do that since we do not create entry<string, hashTable> here.
                                               // The tag is not pre-defined, check if it is registered
                if (tagCategorizedList.ContainsKey(newTag))
                {
                    // change index, continue next line
                    newTagIndex = tagCategorizedList[newTag];
                    return true;
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
                         * is: 1. not predefined; 2. not registered. It will make into uncategorized tag and must create Hashtable here.
                         * However, we may need some change to process projectiles and weapons since they
                         * do not need to "register" in rule files.
                        */
                        newTagIndex = GlobalProperty.SublistIndex.Uncategorized;
                        tagCategorizedList.Add(newTag, newTagIndex);
                        dataSets[(int)newTagIndex].Add(newTag, new Hashtable());
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(inputLine);
                    }
                }
            }
            return true;
        }

        private void MenuItem_Click_file_save(object sender, RoutedEventArgs e)
        {
            /* Write the contents back to the file.
             * However, for debugging purpose, we will write to a separate file.
             */

            // flush the file in exportPath

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlg.ShowDialog();

            string exportPath = dlg.FileName;
            if (exportPath == null)
            {
                return;
            }

            // write first tag, [BuildingTypes]
            StreamWriter sw = new StreamWriter(exportPath, false, Encoding.ASCII);
            List<string> appendixList = new List<string>();

            // Write dummy lines first
            foreach (string line in dummyLinesToWrite)
            {
                sw.WriteLine(line);
            }

            // and write datasets registration list.
            // Weapons and projectiles have no registration list, along side uncategorized list, they must be treated last
            int nonRegisteredListIndex = (int)GlobalProperty.SublistIndex.DummyTags; // 
            int loopIndex;
            for (loopIndex = 0; loopIndex < nonRegisteredListIndex; loopIndex++)
            {
                sw.WriteLine("[" + Enum.GetName(typeof(GlobalProperty.SublistIndex), loopIndex) + "]");
                int j = 1;
                foreach (KeyValuePair<string, Hashtable> item in dataSets[loopIndex])
                {
                    sw.WriteLine(j.ToString() + "=" + item.Key);
                    j++;
                    if (item.Value.Count != 0)
                    {
                        appendixList.Add("[" + item.Key + "]");
                        foreach (DictionaryEntry entry in item.Value)
                        {
                            appendixList.Add((string)entry.Key + "=" + (string)entry.Value);
                        }
                    }
                }
            }

            loopIndex += 1; // Jump over "dummyTag"

            for (; loopIndex < (int)GlobalProperty.SublistIndex.MAX; loopIndex++)
            {
                foreach (KeyValuePair<string, Hashtable> item in dataSets[loopIndex])
                {
                    appendixList.Add("[" + item.Key + "]");
                    foreach (DictionaryEntry entry in item.Value)
                    {
                        appendixList.Add((string)entry.Key + "=" + (string)entry.Value);
                    }
                }
            }

            foreach (string finalWrite in appendixList)
            {
                sw.WriteLine(finalWrite);
            }

            sw.Close();

            // write uncategorized tags for debugging
            StreamWriter swUCT = new StreamWriter(uncategorizedTagExportPath, false, Encoding.ASCII);
            foreach (KeyValuePair<string, Hashtable> item in dataSets[(int)GlobalProperty.SublistIndex.Uncategorized])
            {
                swUCT.WriteLine(item.Key);
            }
            swUCT.Close();

        }

        private void Building_Button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.BuildingTypes);
        }

        private void Open_New_Window(GlobalProperty.SublistIndex categoryIndex)
        {
            CategoryUnitListWindow newWindow = new CategoryUnitListWindow(categoryIndex, this);
            newWindow.Show();
            this.Hide();
        }

        private void Infantry_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.InfantryTypes);
        }

        private void Vehicle_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.VehicleTypes);
        }

        private void Aircraft_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.AircraftTypes);
        }

        private void Warheads_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.Warheads);
        }

        private void Uncategorized_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.Uncategorized);
        }

        private void disable_buttons()
        {
            Building_button.IsEnabled = false;
            Infantry_button.IsEnabled = false;
            Vehicle_button.IsEnabled = false;
            Aircraft_button.IsEnabled = false;
            Warheads_button.IsEnabled = false;
            Uncategorized_button.IsEnabled = false;
            Projectiles_button.IsEnabled = false;
            Weapons_button.IsEnabled = false;
        }

        private void enable_buttons()
        {
            Building_button.IsEnabled = true;
            Infantry_button.IsEnabled = true;
            Vehicle_button.IsEnabled = true;
            Aircraft_button.IsEnabled = true;
            Warheads_button.IsEnabled = true;
            Uncategorized_button.IsEnabled = true;
            Projectiles_button.IsEnabled = true;
            Weapons_button.IsEnabled = true;
        }

        private void postprocessUncategorized()
        {
            // Post-process of uncategorized tags, put them into "weapons" and "projectiles"
            foreach (KeyValuePair<string, Hashtable> item in dataSets[(int)GlobalProperty.SublistIndex.Uncategorized])
            {
                if (item.Value.ContainsKey("Warhead"))
                {
                    tagCategorizedList[item.Key] = GlobalProperty.SublistIndex.Weapons;
                    dataSets[(int)GlobalProperty.SublistIndex.Weapons].Add(item.Key, item.Value);
                    dataSets[(int)GlobalProperty.SublistIndex.Uncategorized].Remove(item.Key);
                    /* This could be tricky. V3 and dreadnought (and some other rocket launchers) use "Warhead=Special";
                     * and other unit like Kirov Airship, use BlimpBombE as ElitePrimay, however, this weapon
                     * uses warhead KTSTLEXP, which collides with an entry in [Animations]: 914=KTSTLEXP.
                     * NOTE: Kirov Airship use BlimpBomb as Primary and it has warhead of BlimpHE, no issue here.
                     * NOTE2: Based on the work of Mamamia of ra2diy.com, I changed warhead of Kirov's ElitePrimary to
                     *        [KTSTLEXPWH], which will avoid naming collision.
                     */
                    string warheadName = (string)item.Value["Warhead"];
                    if (!string.IsNullOrEmpty(warheadName) && warheadName != "Special")
                    {
                        if (tagCategorizedList[warheadName] != GlobalProperty.SublistIndex.Warheads)
                        {
                            tagCategorizedList[warheadName] = GlobalProperty.SublistIndex.Warheads;
                            dataSets[(int)GlobalProperty.SublistIndex.Warheads].Add(warheadName, (dataSets[(int)GlobalProperty.SublistIndex.Uncategorized])[warheadName]);
                            dataSets[(int)GlobalProperty.SublistIndex.Uncategorized].Remove(warheadName);
                        }
                    }

                    try
                    {
                        string projectileName = (string)item.Value["Projectile"];
                        if (!string.IsNullOrEmpty(projectileName))
                        {
                            // non empty projectile field
                            if (tagCategorizedList[projectileName] != GlobalProperty.SublistIndex.Projectiles)
                            {
                                // not registered before
                                tagCategorizedList[projectileName] = GlobalProperty.SublistIndex.Projectiles;
                                dataSets[(int)GlobalProperty.SublistIndex.Projectiles].Add(projectileName, (dataSets[(int)GlobalProperty.SublistIndex.Uncategorized])[projectileName]);
                                dataSets[(int)GlobalProperty.SublistIndex.Uncategorized].Remove(projectileName);
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private void Weapons_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.Weapons);
        }

        private void Projectiles_button_Click(object sender, RoutedEventArgs e)
        {
            Open_New_Window(GlobalProperty.SublistIndex.Projectiles);
        }
    }
}
