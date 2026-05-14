using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SistemaCotizacionesWPF
{
    public partial class CatalogoPrecios : Window
    {
        List<Producto> productos = new List<Producto>();
        List<Proveedor> proveedores = new List<Proveedor>();
        List<Precio> precios = new List<Precio>();
        List<Unidad> unidad = new List<Unidad>();

        List<PrecioVista> listaPrecios = new List<PrecioVista>();

        public CatalogoPrecios()
        {
            InitializeComponent();
            CargarDatos();
        }

        void CargarDatos()
        {
            // PRODUCTOS
            productos.Add(new Producto { Id = 1, Nombre = "Cable" });
            productos.Add(new Producto { Id = 2, Nombre = "Motor" });
            productos.Add(new Producto { Id = 3, Nombre = "Tubería" });
            productos.Add(new Producto { Id = 4, Nombre = "Aceite Industrial" });
            productos.Add(new Producto { Id = 5, Nombre = "Tornillos" });
            productos.Add(new Producto { Id = 6, Nombre = "Cableado" });
            productos.Add(new Producto { Id = 7, Nombre = "Cinta Aislante" });
            productos.Add(new Producto { Id = 8, Nombre = "Acero" });
            productos.Add(new Producto { Id = 9, Nombre = "Sensor" });
            productos.Add(new Producto { Id = 10, Nombre = "Guantes" });

            // PROVEEDORES
            proveedores.Add(new Proveedor { Id = 1, Nombre = "Proveedor A" });
            proveedores.Add(new Proveedor { Id = 2, Nombre = "Proveedor B" });

            // PRECIOS
            precios.Add(new Precio { IdProducto = 1, IdProveedor = 1, Valor = 100 });
            precios.Add(new Precio { IdProducto = 2, IdProveedor = 1, Valor = 500 });
            precios.Add(new Precio { IdProducto = 3, IdProveedor = 1, Valor = 250 });
            precios.Add(new Precio { IdProducto = 4, IdProveedor = 1, Valor = 350 });
            precios.Add(new Precio { IdProducto = 5, IdProveedor = 1, Valor = 50 });
            precios.Add(new Precio { IdProducto = 6, IdProveedor = 1, Valor = 180 });
            precios.Add(new Precio { IdProducto = 7, IdProveedor = 1, Valor = 30 });
            precios.Add(new Precio { IdProducto = 8, IdProveedor = 1, Valor = 900 });
            precios.Add(new Precio { IdProducto = 9, IdProveedor = 1, Valor = 450 });
            precios.Add(new Precio { IdProducto = 10, IdProveedor = 1, Valor = 120 });

            // UNIDADES
            unidad.Add(new Unidad { IdProducto = 1, NombreUnidad = "Metro" });
            unidad.Add(new Unidad { IdProducto = 2, NombreUnidad = "Pieza" });
            unidad.Add(new Unidad { IdProducto = 3, NombreUnidad = "Metro" });
            unidad.Add(new Unidad { IdProducto = 4, NombreUnidad = "Litro" });
            unidad.Add(new Unidad { IdProducto = 5, NombreUnidad = "Caja" });
            unidad.Add(new Unidad { IdProducto = 6, NombreUnidad = "Rollo" });
            unidad.Add(new Unidad { IdProducto = 7, NombreUnidad = "Pieza" });
            unidad.Add(new Unidad { IdProducto = 8, NombreUnidad = "Kilogramo" });
            unidad.Add(new Unidad { IdProducto = 9, NombreUnidad = "Unidad" });
            unidad.Add(new Unidad { IdProducto = 10, NombreUnidad = "Par" });

            comboProducto.ItemsSource = productos;
            comboProducto.DisplayMemberPath = "Nombre";

            comboProveedor.ItemsSource = proveedores;
            comboProveedor.DisplayMemberPath = "Nombre";
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (comboProducto.SelectedItem == null ||
                comboProveedor.SelectedItem == null)
            {
                MessageBox.Show("Selecciona producto y proveedor");
                return;
            }

            var producto = (Producto)comboProducto.SelectedItem;
            var proveedor = (Proveedor)comboProveedor.SelectedItem;

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MessageBox.Show("Precio inválido");
                return;
            }

            listaPrecios.Add(new PrecioVista
            {
                Producto = producto.Nombre ?? "",
                Proveedor = proveedor.Nombre ?? "",
                Precio = precio,
                Unidad = txtUnidad.Text
            });

            gridPrecios.ItemsSource = null;
            gridPrecios.ItemsSource = listaPrecios;

            txtPrecio.Clear();
            txtUnidad.Clear();
        }

        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAgregarCotizacion_Click(object sender, RoutedEventArgs e)
        {
            if (gridPrecios.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un producto");
                return;
            }

            var seleccionado = (PrecioVista)gridPrecios.SelectedItem;

            MainWindow ventanaPrincipal = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.AgregarDesdeCatalogo(
                    seleccionado.Producto,
                    seleccionado.Proveedor,
                    seleccionado.Precio,
                    seleccionado.Unidad
                );

                MessageBox.Show("Producto agregado a la cotización");
            }
        }
    }

    public class PrecioVista
    {
        public string Producto { get; set; } = "";
        public string Proveedor { get; set; } = "";
        public decimal Precio { get; set; }
        public string Unidad { get; set; } = "";
    }
}