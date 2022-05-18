using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //punjenje ComboBox sesijama
            Sesije();
        }

        private void lblX_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void btnPretraga_Click(object sender, RoutedEventArgs e)
        {
            //pretraga se vrsi po bar jednom kriterijumu, u suprotnom se prikazuje upozorenje korisniku
            if (txtGodina.Text != "" || txtNaslovRada.Text.Length >= 3 || cmbSesija.Text != "" || chbObjavljen.IsChecked.Value)
            {
                Pretraga();
            }
            else
            {
                MessageBox.Show("Morate izabrati bar jedan kriterijum pretrage!", "Izaberite kriterijum", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        
        void Pretraga()
        {
            int k = 0;
            txtTabela.Text = "";

            //osnovni SQL upit za pretragu svih radova, treba ga prosiriti kriterijumima za pretragu
            //WHERE 1 je zbog pravilnog formiranja SQL upita kod naknadnog dodavanja kriterijuma pretrage sa AND, 
            //da se ne mora u svakom if bloku ispitivati je li jos nesto selektovano ili nije
            string sql = "SELECT * FROM rad WHERE 1";
            

            //dinamicko generisanje SQL upita, zavisno od unesenih vrijednosti za pretragu
            //ista funkcionalnost se moze realizovati i dodavanjem parametara SQL komandi, prethodno se mora kreirati komanda bez SQL upita, pa ga tek nakon generisanja upita dodijeliti komandi
            if (txtGodina.Text != "")
            {
                sql += " AND simpozijum='" + txtGodina.Text + "'";
            }
            if (cmbSesija.Text != "")
            {
                sql += " AND sesija='" + cmbSesija.Text + "'";
            }
            if (txtNaslovRada.Text.Length >=3)
            {
                sql += " AND naslov like '%" + txtNaslovRada.Text + "%'";
            }
            if (chbObjavljen.IsChecked.Value)
            {
                sql += " AND objavljen='1'";
            }

            //sortiranje podataka
            sql += " ORDER BY sesija, rb";

            MySqlConnection con = new MySqlConnection("Server=localhost;database=tmp_jun1;uid=root;pwd=;");
            MySqlCommand cmd = new MySqlCommand(sql, con);
            con.Open();
            MySqlDataReader reader = cmd.ExecuteReader();

            //formiranje stringa za zaglavlje tabele, pomocu tabova za razmak kolona (\t) i novog reda na kraju (\n)
            txtTabela.Text = "ID\tSimpozijum\tSesija\tRB\tPoster\tVirtual\tNaslov\n";
            //obrada rezultata upita
            while (reader.Read())
            {
                string id = reader["id"].ToString();
                string simpozijum = reader["simpozijum"].ToString();
                string sesija = reader["sesija"].ToString();
                string rb = reader["rb"].ToString();
                string poster = (bool)reader["poster"] ? "DA" : "NE";
                string virt = (bool)reader["virtual"] ? "DA" : "NE";
                string naslov = reader["naslov"].ToString();
                //formiranje stringa za prikaz u tabelarnoj formi, pomocu tabova (\t) i novog reda na kraju (\n)
                txtTabela.Text += String.Format("{0}\t{1}\t\t{2}\t{3}\t{4}\t{5}\t{6}\n", id, simpozijum, sesija, rb, poster, virt, naslov);
                k++;
            }
            reader.Close();
            con.Close();
            lblBrRadova.Content = "Broj radova: " + k;
        }

        void Sesije()
        {

            //SQL upit za dobijanje razlicitih sesija, sortirano po sesiji rastucim redoslijedom (ASC se podrazumijeva)
            string sql = "SELECT DISTINCT sesija FROM rad ORDER BY sesija";
            MySqlConnection con = new MySqlConnection("Server=localhost;database=tmp_jun1;uid=root;pwd=;");
            MySqlCommand cmd = new MySqlCommand(sql, con);
            con.Open();
            MySqlDataReader reader = cmd.ExecuteReader();

            //punjenje ComboBox sesija, inicijalno posjeduje jednu praznu stavku, da se moze eliminisati iz pretrage
            while (reader.Read())
            {
                string sesija = reader["sesija"].ToString();
                cmbSesija.Items.Add(sesija);
            }
            reader.Close();
            con.Close();
        }

        private void btnPonisti_Click(object sender, RoutedEventArgs e)
        {
            txtGodina.Clear();
            txtNaslovRada.Clear();
            lblBrRadova.Content = "";
            cmbSesija.SelectedIndex = -1;
            chbObjavljen.IsChecked = false;
            txtTabela.Text = "";
        }
    }
}
