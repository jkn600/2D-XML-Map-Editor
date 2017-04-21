using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using System.Collections.ObjectModel;
using System.Xml;

//Tile Class (with features)
public class Tiles
{
    //Class Constructor
    public Tiles() { }

    //Getters
    public int getColumns
    {
        get
        {
            return columns;
        }
    }

    public int getRows
    {
        get
        {
            return rows;
        }
    }

    public int getTileSize
    {
        get
        {
            return tileSize;
        }
    }

    //Variables
    private const int columns = 20;

    private const int rows = 9;

    private const int tileSize = 32;

    //Methods
    //Load the Tile List
    public void tileLoad(ObservableCollection<Image> mapImage, ObservableCollection<Image> tileImage)
    {
        if (mapImage.Count >= 1)
        {
            System.Windows.MessageBox.Show("Please clear the current map first.", "Cyclical Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else if (tileImage.Count < 1)
        {
            createTilesList(tileImage, rows, columns, tileSize);
        }
        else if (tileImage.Count >= 1)
        {
            tileImage.Clear();
            createTilesList(tileImage, rows, columns, tileSize);
        }
    }

    // generate the set of tiles from the file containing all the tiles
    public void createTilesList(ObservableCollection<Image> tileImage, int rows, int columns, int tileSize)
    {
        // Configure open file dialog box
        Microsoft.Win32.OpenFileDialog loadImage = new Microsoft.Win32.OpenFileDialog();
        loadImage.FileName = ""; // Default file name
        loadImage.DefaultExt = ".bmp"; // Default file extension
        loadImage.Filter = "Tile sheet (.bmp)|*.bmp"; // Filter files by extension 

        // Show open file dialog box
        Nullable<bool> result = loadImage.ShowDialog();

        // Process open file dialog box results 
        if (result == true)
        {
            // Open document 
            string image = loadImage.FileName;

            // loop through all the tiles and add them to the set of tiles in the right handside listview
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    // instantiate a new Image as a CroppedBitmap from the loaded file
                    tileImage.Add(new Image()
                    {
                        Source = new CroppedBitmap(new BitmapImage(new Uri(image, UriKind.Relative)),
                            new Int32Rect(j * tileSize, i * tileSize, tileSize, tileSize)),
                        Height = tileSize
                    });
                }

        }
    }

}

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //variable declaration
        bool isSaved = true; //Determines whether the current project is saved or not
        bool changesMade = false; //Determines whether changes have been made to the current project

        //Tiles Class Object
        Tiles tile = new Tiles();

        // Automatically updates the UI element it is bound to 
        ObservableCollection<Image> tileImages = new ObservableCollection<Image>();
        ObservableCollection<Image> mapImages = new ObservableCollection<Image>();

        List<int> tileIndexes = new List<int>();

        #region WINDOW_MANAGEMENT

        public MainWindow()
        {
            // main initialisation of the GUI components
            InitializeComponent();

            #region IMAGES

            // initialise the source of tiles
            tileList.ItemsSource = tileImages;
            // initialise the source of map's tiles
            map.ItemsSource = mapImages;

            #endregion
        }

        //Show the main Window
        private void createWindow()
        {

            new MainWindow().Show();
        }

        //restrict resizing to one axis
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MinWidth = this.ActualWidth;
            this.MaxWidth = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
        }

        private void about_Window(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Cyclical Editor v0.0.3. Copyright Cyclical Gaming, All Rights Reserved.", "About");
        }

        #endregion

        #region TILES

        //add the tiles to the set of tiles
        private void loadTileSet(object sender, RoutedEventArgs e)
        {
            tile.tileLoad(mapImages, tileImages);
        }

        #endregion

        #region ADDTILES

        // add the selected tile to the map
        private void btn_AddTileToMap(object sender, RoutedEventArgs e)
        {
            if (tileList.SelectedItem != null)
            {
                mapImages.Add(new Image() { Source = ((Image)tileList.SelectedItem).Source, Height = tile.getTileSize });
                tileIndexes.Add(tileList.SelectedIndex);
            }
            changesMade = true;
            isSaved = false;
        }

        private void btn_DeleteTile(object sender, RoutedEventArgs e)
        {
            int lastTile = mapImages.Count - 1;

            if(mapImages.Count > 0)
            {
                mapImages.RemoveAt(lastTile);
                tileIndexes.RemoveAt(lastTile);
            }
            else
            {

            }
            changesMade = true;
            isSaved = false;
        }

        #endregion

        #region MAP_MANAGEMENT

        private void loadMap(object sender, RoutedEventArgs e)
        {
            //startAnew(sender, e);
            if (tileImages.Count < 1)
            {
                MessageBoxResult OK = System.Windows.MessageBox.Show("Please load a tile set first", "Cyclical Editor", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (OK == MessageBoxResult.OK)
                {
                    loadTileSet(sender, e);
                }
            }
            else if (mapImages.Count < 1)
            {
                //System.Windows.MessageBox.Show("Please load the corresponding tileset that this map will use first.");
                MessageBoxResult OKCancel = System.Windows.MessageBox.Show("Ensure the map you are loading uses the currently imported tile set, as loading a map with the wrong tile set imported will lead to the wrong map being generated, would you like to continue?",
                    "Cyclical Editor", System.Windows.MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (OKCancel == MessageBoxResult.OK)
                {
                    Map_Load_Click(sender, e);
                }
                else if (OKCancel == MessageBoxResult.Cancel)
                {
                    return;
                }
                changesMade = false;
                return;
            }
            else if (mapImages.Count >= 1)
            {
                System.Windows.MessageBox.Show("Please clear or save the current map before loading a new one.", "Cyclical Editor", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void clearMap(object sender, RoutedEventArgs e)
        {
            if (mapImages.Count >= 1)
            {
                if (changesMade == true && isSaved == false)
                {
                    MessageBoxResult YesNoCancel = System.Windows.MessageBox.Show("Do you wish to save the current map?", "Cyclical Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (YesNoCancel == MessageBoxResult.Yes)
                    {
                        Map_Save_Click(sender, e);
                        if (isSaved == true)
                        {
                            mapImages.Clear();
                            tileIndexes.Clear();
                            changesMade = false;
                        }
                    }
                    else if (YesNoCancel == MessageBoxResult.No)
                    {
                        mapImages.Clear();
                        tileIndexes.Clear();
                        changesMade = false;
                        isSaved = true;
                    }
                    else if (YesNoCancel == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                else
                {
                    mapImages.Clear();
                    tileIndexes.Clear();
                    changesMade = false;
                    isSaved = true;
                }
            }
        }

        #endregion

        #region FILEMANAGEMENT
        
        //Load Map
        private void Map_Load_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            //TODO CHANGE LOAD TO PROJECT FILES

            //Open Dialog Box
            Microsoft.Win32.OpenFileDialog loadbox = new Microsoft.Win32.OpenFileDialog();

            XmlDocument xmlDoc = new XmlDocument ();

            loadbox.FileName = ""; //Default name
            loadbox.DefaultExt = ".xml"; //Default Extension
            loadbox.Filter = "Editor Files (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = loadbox.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                
               // Open document 
                    filename = loadbox.FileName;
                    xmlDoc.Load(filename);
                    changesMade = false;
                    isSaved = false;
            }
            if (xmlDoc.DocumentElement != null)
            {
                // get root
                XmlNode root = xmlDoc.DocumentElement;
                var xmlRootChildren = root.FirstChild;
                string tiles = xmlRootChildren.InnerText;

                //split the tiles
                string[] split = tiles.Split(new Char[] { ',' });
                List<int> proper = new List<int>();

                for (int i = 0; i < split.Count() - 1; i++)
                {
                    proper.Add(int.Parse(split[i]));
                    tileIndexes.Add(int.Parse(split[i]));
                }

                for (int j = 0; j < proper.Count; j++)
                {
                    tileList.SelectedIndex = proper[j];
                    if (mapImages.Count < j + 1 )
                    {
                        mapImages.Add(new Image() { Source = ((Image)tileList.SelectedItem).Source, Height = tile.getTileSize });
                    }

                }
            }

        }

        //Save Map
        private void Map_Save_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog savebox = new Microsoft.Win32.SaveFileDialog();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("Map");
            //TODO: LOOKUP Serialize - System.Xml.Serialization.XmlSerializer serialCreater = new System.Xml.Serialization.XmlSerializer(tileIndexes.GetType());
            xmlDoc.AppendChild(root);

            // make map node
            // append map node to root
            XmlNode map = xmlDoc.CreateElement("mapData");


            //saveMapData(root, map);

            //make string
            string mapData = string.Empty;

            for (int i = 0; i < tileIndexes.Count; i++)
            {
                mapData += tileIndexes[i] + ",";
            }

            map.InnerText = mapData;
            root.AppendChild(map);


            // make a loop
            // for each element in maplist
            // add $value, to string


            savebox.FileName = ""; // Default file name
            savebox.DefaultExt = ".xml"; // Default file extension
            savebox.Filter = "Editor Files (.xml)|*.xml"; // Filter files by extension 

            // Show save file dialog box
            Nullable<bool> result = savebox.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Save document 
                filename = savebox.FileName;
                xmlDoc.Save(filename);
                isSaved = true;
                changesMade = false;

            }

            // save the actual contents of the config file
            // - open the file for writing
            // - write the actual contents
            // - (don't forget to) close the file


        }

        //Starting a New Project
        private void startAnew(object sender, RoutedEventArgs e)
        {
            if (changesMade == true && isSaved == false)
            {
                //Prompt user to save current project before continuing
                MessageBoxResult YesNoCancel = System.Windows.MessageBox.Show("Save changes to the current project?",
                    "Cyclical Editor", System.Windows.MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (YesNoCancel == MessageBoxResult.Yes) //If the user clicks yes, the saveBoxDialog opens and allows them to save a file, if the user cancels, nothing happens
                {
                    Map_Save_Click(sender, e);
                    if (isSaved == true)
                    {
                        tileImages.Clear();
                        tileIndexes.Clear();
                        mapImages.Clear();
                        isSaved = false;
                    }
                    return;
                }
                else if (YesNoCancel == MessageBoxResult.No) //If the user chooses not to save and continue, the program clears all lists and starts a new project
                {
                    tileImages.Clear();
                    tileIndexes.Clear();
                    mapImages.Clear();
                    isSaved = false;
                    changesMade = false;
                    return;

                }
                else if (YesNoCancel == MessageBoxResult.Cancel) //If the user cancels, the program does nothing
                {

                }
                return;
            }
            else
            {
                tileImages.Clear();
                tileIndexes.Clear();
                mapImages.Clear();
                isSaved = false;
                changesMade = false;
                return;
            }
        }

        #region project file work - TODO's

        //Save Project
        private void projectSave(object sender, RoutedEventArgs e)
        {
            string filename = "";

            //Configure Save File Dialog Box
            Microsoft.Win32.SaveFileDialog saver = new Microsoft.Win32.SaveFileDialog();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode root = xmlDoc.CreateElement("Project");
            xmlDoc.AppendChild(root);

            //make project node
            //append project node to root

            XmlNode project = xmlDoc.CreateElement("projectData");

            saveMapData(root, project);

            saver.FileName = ""; // Default file name
            saver.DefaultExt = ".cge"; // Default file extension
            saver.Filter = "Project Files (.cge)|*.cge"; // Filter files by extension 

            Nullable<bool> result = saver.ShowDialog();

            if (result == true)
            {
                // Save document 
                filename = saver.FileName;
                xmlDoc.Save(filename);
                isSaved = true;
                changesMade = false;
            }
        }

        //Load Project
        private void projectLoad(object sender, RoutedEventArgs e)
        {

        }

        //Extracted the saving of mapData for use in multiple methods, moreso project files
        private void saveMapData(XmlNode root, XmlNode data)
        {
            //make string
            string mapData = string.Empty;

            for (int i = 0; i < tileIndexes.Count; i++)
            {
                mapData += tileIndexes[i] + ",";
            }

            data.InnerText = mapData;
            root.AppendChild(data);
        }

        #endregion

        #endregion

    }
}
