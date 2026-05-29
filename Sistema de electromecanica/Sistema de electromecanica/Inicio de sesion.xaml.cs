using SistemaCotizacionesWPF;
using System.Windows;

namespace Sistema_de_electromecanica
{
    public partial class Inicio_de_sesion : Window
    {
        public Inicio_de_sesion()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            if (usuario == "admin" && password == "1234")
            {
                MainWindow ventana = new MainWindow();
                ventana.Show();

                this.Close();
            }
            else
            {
                lblError.Text = "Usuario o contraseña incorrectos";
            }
        }
    }
}