using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;

namespace SistemaCotizacionesWPF
{
    public class Producto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class Proveedor
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class Precio
    {
        public int IdProducto { get; set; }
        public int IdProveedor { get; set; }
        public decimal Valor { get; set; }
    }

    public class Unidad
    {
        public int IdProducto { get; set; }
        public string? NombreUnidad { get; set; }
    }

    public class ItemCotizacion
    {
        public string? Producto { get; set; }
        public string? Proveedor { get; set; }
        public int Cantidad { get; set; }
        public string? Unidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class MaterialAPU
    {
        public string? Material { get; set; }
        public string? NumeroSerie { get; set; }
        public string? Unidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }

    // ── Modelo para la tabla de presupuesto ──────────────────
    public class ItemPresupuesto
    {
        public int No { get; set; }
        public string? Concepto { get; set; }
        public string? Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PU { get; set; }
        public decimal Importe => PU * Cantidad;
    }

    public partial class MainWindow : Window
    {
        // Listas cotización
        private readonly List<Producto> productos = new();
        private readonly List<Proveedor> proveedores = new();
        private readonly List<Precio> precios = new();
        private readonly List<Unidad> unidades = new();
        private readonly List<ItemCotizacion> items = new();
        private decimal total = 0;

        // Listas APU
        private readonly List<MaterialAPU> catalogoMateriales = new();
        private readonly List<MaterialAPU> catalogoManoObra = new();
        private readonly List<MaterialAPU> catalogoEquipo = new();
        private readonly List<MaterialAPU> materialesSeleccionados = new();
        private readonly List<MaterialAPU> filasManoObra = new();
        private readonly List<MaterialAPU> filasEquipo = new();

        // Lista presupuesto
        private readonly List<ItemPresupuesto> itemsPresupuesto = new();

        public MainWindow()
        {
            InitializeComponent();
            CargarDatos();
            InicializarAPU();
        }

        // ── Carga de datos Cotización ─────────────────────────
        private void CargarDatos()
        {
            productos.AddRange(new[]
            {
                new Producto { Id = 1,  Nombre = "Cable"            },
                new Producto { Id = 2,  Nombre = "Motor"            },
                new Producto { Id = 3,  Nombre = "Tubería"          },
                new Producto { Id = 4,  Nombre = "Aceite Industrial" },
                new Producto { Id = 5,  Nombre = "Tornillos"        },
                new Producto { Id = 6,  Nombre = "Cableado"         },
                new Producto { Id = 7,  Nombre = "Cinta Aislante"   },
                new Producto { Id = 8,  Nombre = "Acero"            },
                new Producto { Id = 9,  Nombre = "Sensor"           },
                new Producto { Id = 10, Nombre = "Guantes"          }
            });

            proveedores.AddRange(new[]
            {
                new Proveedor { Id = 1, Nombre = "Proveedor A" },
                new Proveedor { Id = 2, Nombre = "Proveedor B" }
            });

            precios.AddRange(new[]
            {
                new Precio { IdProducto = 1,  IdProveedor = 1, Valor = 100 },
                new Precio { IdProducto = 2,  IdProveedor = 1, Valor = 500 },
                new Precio { IdProducto = 3,  IdProveedor = 1, Valor = 250 },
                new Precio { IdProducto = 4,  IdProveedor = 1, Valor = 350 },
                new Precio { IdProducto = 5,  IdProveedor = 1, Valor = 50  },
                new Precio { IdProducto = 6,  IdProveedor = 1, Valor = 180 },
                new Precio { IdProducto = 7,  IdProveedor = 1, Valor = 30  },
                new Precio { IdProducto = 8,  IdProveedor = 1, Valor = 900 },
                new Precio { IdProducto = 9,  IdProveedor = 1, Valor = 450 },
                new Precio { IdProducto = 10, IdProveedor = 1, Valor = 120 }
            });

            unidades.AddRange(new[]
            {
                new Unidad { IdProducto = 1,  NombreUnidad = "Metro"     },
                new Unidad { IdProducto = 2,  NombreUnidad = "Pieza"     },
                new Unidad { IdProducto = 3,  NombreUnidad = "Metro"     },
                new Unidad { IdProducto = 4,  NombreUnidad = "Litro"     },
                new Unidad { IdProducto = 5,  NombreUnidad = "Caja"      },
                new Unidad { IdProducto = 6,  NombreUnidad = "Rollo"     },
                new Unidad { IdProducto = 7,  NombreUnidad = "Pieza"     },
                new Unidad { IdProducto = 8,  NombreUnidad = "Kilogramo" },
                new Unidad { IdProducto = 9,  NombreUnidad = "Unidad"    },
                new Unidad { IdProducto = 10, NombreUnidad = "Par"       }
            });

            comboProducto.ItemsSource = productos;
            comboProducto.DisplayMemberPath = "Nombre";
            comboProveedor.ItemsSource = proveedores;
            comboProveedor.DisplayMemberPath = "Nombre";

            comboProducto.SelectionChanged += ActualizarPrecio;
            comboProveedor.SelectionChanged += ActualizarPrecio;
        }

        // ── Helpers Cotización ────────────────────────────────
        private void RefrescarGrid()
        {
            grid.ItemsSource = null;
            grid.ItemsSource = items;
            lblTotal.Text = $"Total: ${total:0.00}";
        }

        private Cell CreateCell(string text, iTextTextAlignment alignment) =>
            new Cell()
                .Add(new Paragraph(text).SetFontSize(10))
                .SetTextAlignment(alignment)
                .SetPadding(6)
                .SetBorder(new SolidBorder(1));

        private Cell CreateCellSm(string text, iTextTextAlignment alignment) =>
            new Cell()
                .Add(new Paragraph(text).SetFontSize(9))
                .SetTextAlignment(alignment)
                .SetPadding(4)
                .SetBorder(new SolidBorder(1));

        // ── Eventos Cotización ────────────────────────────────
        private void ActualizarPrecio(object sender, EventArgs e)
        {
            if (comboProducto.SelectedItem is not Producto producto ||
                comboProveedor.SelectedItem is not Proveedor proveedor)
                return;

            var precio = precios.FirstOrDefault(p =>
                p.IdProducto == producto.Id &&
                p.IdProveedor == proveedor.Id);

            txtPrecio.Text = precio?.Valor.ToString() ?? "0";
        }

        private void BtnAbrirCatalogo_Click(object sender, RoutedEventArgs e)
        {
            new CatalogoPrecios().Show();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (comboProducto.SelectedItem is not Producto producto ||
                comboProveedor.SelectedItem is not Proveedor proveedor)
                return;

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MessageBox.Show("Precio inválido");
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad inválida");
                return;
            }

            var unidadProducto = unidades.FirstOrDefault(u => u.IdProducto == producto.Id);
            decimal subtotal = precio * cantidad;
            total += subtotal;

            items.Add(new ItemCotizacion
            {
                Producto = producto.Nombre,
                Proveedor = proveedor.Nombre,
                Cantidad = cantidad,
                Unidad = unidadProducto?.NombreUnidad ?? "N/A",
                Precio = precio,
                Subtotal = subtotal
            });

            RefrescarGrid();
            txtCantidad.Clear();
        }

        public void AgregarDesdeCatalogo(
            string producto, string proveedor,
            decimal precio, string unidadTexto)
        {
            total += precio;
            items.Add(new ItemCotizacion
            {
                Producto = producto,
                Proveedor = proveedor,
                Cantidad = 1,
                Unidad = unidadTexto,
                Precio = precio,
                Subtotal = precio
            });
            RefrescarGrid();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = grid.SelectedItems
                .Cast<ItemCotizacion>()
                .ToList();

            foreach (var item in seleccionados)
            {
                total -= item.Subtotal;
                items.Remove(item);
            }

            RefrescarGrid();
        }

        // ── Exportar PDF Cotización ───────────────────────────
        private void BtnPDF_Click(object sender, RoutedEventArgs e)
        {
            if (items.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar");
                return;
            }

            var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
            if (dialog.ShowDialog() != true) return;

            var itemsSnapshot = items.ToList();
            decimal totalSnapshot = total;
            string ruta = dialog.FileName;

            using (PdfWriter writer = new PdfWriter(ruta))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.SetMargins(25, 25, 25, 25);

                // Encabezado empresa
                document.Add(new Paragraph("EMPRESA INDUSTRIAL MX")
                    .SetFontSize(20)
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

                document.Add(new Paragraph("COTIZACIÓN")
                    .SetFontSize(18)
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

                // Datos del cliente desde los TextBox del XAML
                string nombre = string.IsNullOrWhiteSpace(txtClienteNombre?.Text)
                    ? "____________________________" : txtClienteNombre!.Text;
                string empresa = string.IsNullOrWhiteSpace(txtClienteEmpresa?.Text)
                    ? "____________________________" : txtClienteEmpresa!.Text;
                string telefono = string.IsNullOrWhiteSpace(txtClienteTelefono?.Text)
                    ? "____________________________" : txtClienteTelefono!.Text;
                string correo = string.IsNullOrWhiteSpace(txtClienteCorreo?.Text)
                    ? "____________________________" : txtClienteCorreo!.Text;

                document.Add(new Paragraph("Datos del Cliente:").SetFontSize(12));
                document.Add(new Paragraph($"Nombre:   {nombre}"));
                document.Add(new Paragraph($"Empresa:  {empresa}"));
                document.Add(new Paragraph($"Teléfono: {telefono}"));
                document.Add(new Paragraph($"Correo:   {correo}"));
                document.Add(new Paragraph(" "));

                // Tabla de productos
                float[] colWidths = { 150, 150, 80, 80, 100, 100 };
                var table = new Table(UnitValue.CreatePointArray(colWidths))
                    .UseAllAvailableWidth();

                foreach (var h in new[] { "Producto", "Proveedor", "Cantidad", "Unidad", "Precio", "Subtotal" })
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFontSize(11))
                        .SetBackgroundColor(new DeviceRgb(70, 70, 70))
                        .SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(iTextTextAlignment.CENTER)
                        .SetPadding(8)
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                }

                foreach (var item in itemsSnapshot)
                {
                    table.AddCell(CreateCell(item.Producto ?? "", iTextTextAlignment.LEFT));
                    table.AddCell(CreateCell(item.Proveedor ?? "", iTextTextAlignment.LEFT));
                    table.AddCell(CreateCell(item.Cantidad.ToString(), iTextTextAlignment.CENTER));
                    table.AddCell(CreateCell(item.Unidad ?? "", iTextTextAlignment.CENTER));
                    table.AddCell(CreateCell($"${item.Precio:0.00}", iTextTextAlignment.RIGHT));
                    table.AddCell(CreateCell($"${item.Subtotal:0.00}", iTextTextAlignment.RIGHT));
                }

                document.Add(table);
                document.Add(new Paragraph(" "));

                // Total
                var totalTable = new Table(new float[] { 400, 100 });
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph("SUBTOTAL:").SetFontSize(11))
                    .SetTextAlignment(iTextTextAlignment.RIGHT).SetPadding(6));
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${totalSnapshot:0.00}").SetFontSize(11))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230)).SetPadding(6));

                decimal ivaTotal = totalSnapshot * 0.16m;
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph("IVA (16%):").SetFontSize(11))
                    .SetTextAlignment(iTextTextAlignment.RIGHT).SetPadding(6));
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${ivaTotal:0.00}").SetFontSize(11))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230)).SetPadding(6));

                decimal totalConIva = totalSnapshot + ivaTotal;
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph("TOTAL:").SetFontSize(13))
                    .SetTextAlignment(iTextTextAlignment.RIGHT).SetPadding(6));
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${totalConIva:0.00}").SetFontSize(13))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(new DeviceRgb(200, 200, 200)).SetPadding(6));

                document.Add(totalTable);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                document.Add(new Paragraph("____________________________________")
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph("Firma autorizada")
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Gracias por su confianza")
                    .SetFontSize(9)
                    .SetTextAlignment(iTextTextAlignment.CENTER)
                    .SetFontColor(new DeviceRgb(128, 128, 128)));
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ruta,
                UseShellExecute = true
            });

            MessageBox.Show("PDF generado correctamente");
        }

        // ══════════════════════════════════════════════════════
        //                      APU
        // ══════════════════════════════════════════════════════

        private void InicializarAPU()
        {
            // ── Catálogo Materiales ──
            catalogoMateriales.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "CAB-001", Material = "Cable THW calibre 12",          PrecioUnitario = 18,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "CAB-002", Material = "Cable THW calibre 10",          PrecioUnitario = 28,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "TUB-001", Material = "Tubería conduit 3/4\"",         PrecioUnitario = 65,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "PRO-001", Material = "Interruptor termomagnético 1P", PrecioUnitario = 180,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MOT-001", Material = "Motor trifásico 1HP",           PrecioUnitario = 2800, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "SEN-001", Material = "Sensor de temperatura PT100",   PrecioUnitario = 380,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MAT-001", Material = "Cinta aislante 3M",             PrecioUnitario = 35,   Unidad = "Rollo" },
                new MaterialAPU { NumeroSerie = "MAT-002", Material = "Tornillos industriales",        PrecioUnitario = 80,   Unidad = "Caja"  }
            });

            // ── Catálogo Mano de Obra precargado ──
            catalogoManoObra.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "MO-001", Material = "Instalación eléctrica general",       PrecioUnitario = 500,  Unidad = "Lote" },
                new MaterialAPU { NumeroSerie = "MO-002", Material = "Desinstalación de arbotante 7.5 mts", PrecioUnitario = 500,  Unidad = "Lote" },
                new MaterialAPU { NumeroSerie = "MO-003", Material = "Desinstalación de luminaria",        PrecioUnitario = 400,  Unidad = "Lote" },
                new MaterialAPU { NumeroSerie = "MO-004", Material = "Instalación de luminaria LED",       PrecioUnitario = 350,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MO-005", Material = "Conexión de tablero eléctrico",      PrecioUnitario = 800,  Unidad = "Lote" },
                new MaterialAPU { NumeroSerie = "MO-006", Material = "Tendido de cable",                   PrecioUnitario = 15,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "MO-007", Material = "Instalación de motor eléctrico",     PrecioUnitario = 1200, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MO-008", Material = "Mantenimiento preventivo motor",     PrecioUnitario = 600,  Unidad = "Hora" }
            });

            // ── Catálogo Equipo y Herramienta precargado ──
            catalogoEquipo.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "EQ-001", Material = "Grúa con canastilla",        PrecioUnitario = 970,  Unidad = "Hr"   },
                new MaterialAPU { NumeroSerie = "EQ-002", Material = "Andamio tubular",            PrecioUnitario = 180,  Unidad = "Día"  },
                new MaterialAPU { NumeroSerie = "EQ-003", Material = "Taladro industrial",         PrecioUnitario = 120,  Unidad = "Hr"   },
                new MaterialAPU { NumeroSerie = "EQ-004", Material = "Multímetro digital",         PrecioUnitario = 50,   Unidad = "Día"  },
                new MaterialAPU { NumeroSerie = "EQ-005", Material = "Escalera dieléctrica 6 mts", PrecioUnitario = 80,   Unidad = "Día"  },
                new MaterialAPU { NumeroSerie = "EQ-006", Material = "Generador eléctrico",        PrecioUnitario = 450,  Unidad = "Hr"   },
                new MaterialAPU { NumeroSerie = "EQ-007", Material = "Camión de servicio",         PrecioUnitario = 600,  Unidad = "Día"  }
            });

            gridCatalogoMateriales.ItemsSource = catalogoMateriales;
            gridMaterialesSeleccionados.ItemsSource = materialesSeleccionados;
            gridCatalogoManoObra.ItemsSource = catalogoManoObra;
            gridManoObra.ItemsSource = filasManoObra;
            gridCatalogoEquipo.ItemsSource = catalogoEquipo;
            gridEquipo.ItemsSource = filasEquipo;
            gridPresupuesto.ItemsSource = itemsPresupuesto;
        }

        // ── Agregar/Quitar Materiales ─────────────────────────
        private void BtnAgregarMaterialAPU_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoMateriales.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un material del catálogo");
                return;
            }

            if (!int.TryParse(txtCantidadMaterial.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida");
                return;
            }

            var existente = materialesSeleccionados
                .FirstOrDefault(m => m.NumeroSerie == seleccionado.NumeroSerie);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.Subtotal = existente.PrecioUnitario * existente.Cantidad;
            }
            else
            {
                materialesSeleccionados.Add(new MaterialAPU
                {
                    NumeroSerie = seleccionado.NumeroSerie,
                    Material = seleccionado.Material,
                    Unidad = seleccionado.Unidad,
                    PrecioUnitario = seleccionado.PrecioUnitario,
                    Cantidad = cantidad,
                    Subtotal = seleccionado.PrecioUnitario * cantidad
                });
            }

            RefrescarGridSeleccionados();
            CalcularAPU();
            txtCantidadMaterial.Text = "1";
        }

        private void BtnQuitarMaterialAPU_Click(object sender, RoutedEventArgs e)
        {
            if (gridMaterialesSeleccionados.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un material de la lista");
                return;
            }

            materialesSeleccionados.Remove(seleccionado);
            RefrescarGridSeleccionados();
            CalcularAPU();
        }

        private void RefrescarGridSeleccionados()
        {
            gridMaterialesSeleccionados.ItemsSource = null;
            gridMaterialesSeleccionados.ItemsSource = materialesSeleccionados;
        }

        // ── Agregar/Quitar Mano de Obra ───────────────────────
        private void BtnAgregarManoObra_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoManoObra.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un concepto del catálogo de mano de obra");
                return;
            }

            if (!decimal.TryParse(txtCantidadManoObra.Text, out decimal cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida");
                return;
            }

            var existente = filasManoObra
                .FirstOrDefault(m => m.NumeroSerie == seleccionado.NumeroSerie);

            if (existente != null)
            {
                existente.Cantidad += (int)cantidad;
                existente.Subtotal = existente.PrecioUnitario * existente.Cantidad;
            }
            else
            {
                filasManoObra.Add(new MaterialAPU
                {
                    NumeroSerie = seleccionado.NumeroSerie,
                    Material = seleccionado.Material,
                    Unidad = seleccionado.Unidad,
                    PrecioUnitario = seleccionado.PrecioUnitario,
                    Cantidad = (int)cantidad,
                    Subtotal = seleccionado.PrecioUnitario * (int)cantidad
                });
            }

            RefrescarGridManoObra();
            CalcularAPU();
            txtCantidadManoObra.Text = "1";
        }

        private void BtnQuitarManoObra_Click(object sender, RoutedEventArgs e)
        {
            if (gridManoObra.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un concepto de la lista");
                return;
            }

            filasManoObra.Remove(seleccionado);
            RefrescarGridManoObra();
            CalcularAPU();
        }

        private void RefrescarGridManoObra()
        {
            gridManoObra.ItemsSource = null;
            gridManoObra.ItemsSource = filasManoObra;
        }

        // ── Agregar/Quitar Equipo ─────────────────────────────
        private void BtnAgregarEquipo_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoEquipo.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un equipo del catálogo");
                return;
            }

            if (!decimal.TryParse(txtCantidadEquipo.Text, out decimal cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad válida");
                return;
            }

            var existente = filasEquipo
                .FirstOrDefault(m => m.NumeroSerie == seleccionado.NumeroSerie);

            if (existente != null)
            {
                existente.Cantidad += (int)cantidad;
                existente.Subtotal = existente.PrecioUnitario * existente.Cantidad;
            }
            else
            {
                filasEquipo.Add(new MaterialAPU
                {
                    NumeroSerie = seleccionado.NumeroSerie,
                    Material = seleccionado.Material,
                    Unidad = seleccionado.Unidad,
                    PrecioUnitario = seleccionado.PrecioUnitario,
                    Cantidad = (int)cantidad,
                    Subtotal = seleccionado.PrecioUnitario * (int)cantidad
                });
            }

            RefrescarGridEquipo();
            CalcularAPU();
            txtCantidadEquipo.Text = "1";
        }

        private void BtnQuitarEquipo_Click(object sender, RoutedEventArgs e)
        {
            if (gridEquipo.SelectedItem is not MaterialAPU seleccionado)
            {
                MessageBox.Show("Selecciona un equipo de la lista");
                return;
            }

            filasEquipo.Remove(seleccionado);
            RefrescarGridEquipo();
            CalcularAPU();
        }

        private void RefrescarGridEquipo()
        {
            gridEquipo.ItemsSource = null;
            gridEquipo.ItemsSource = filasEquipo;
        }

        // ── CellEditEnding para escritura directa ─────────────
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(CalcularAPU),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        // ── Calcular ──────────────────────────────────────────
        private void BtnCalcularAPU_Click(object sender, RoutedEventArgs e)
        {
            CalcularAPU();
            GenerarPresupuesto();
        }

        private void TxtFactor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblResTotal == null ||
                lblResIva == null ||
                lblTotalConIva == null ||
                lblCostoDirecto == null)
                return;

            CalcularAPU();
        }

        private void CalcularAPU()
        {
            foreach (var f in filasManoObra)
                f.Subtotal = f.PrecioUnitario * f.Cantidad;
            foreach (var f in filasEquipo)
                f.Subtotal = f.PrecioUnitario * f.Cantidad;

            decimal subMat = materialesSeleccionados.Sum(m => m.Subtotal);
            decimal subMO = filasManoObra.Sum(f => f.Subtotal);
            decimal subEq = filasEquipo.Sum(f => f.Subtotal);
            decimal cd = subMat + subMO + subEq;

            double pctMat = cd > 0 ? (double)(subMat / cd * 100) : 0;
            double pctMO = cd > 0 ? (double)(subMO / cd * 100) : 0;
            double pctEq = cd > 0 ? (double)(subEq / cd * 100) : 0;

            decimal pInd = decimal.TryParse(txtApuIndirectos.Text, out decimal ind) ? ind / 100 : 0;
            decimal pFin = decimal.TryParse(txtApuFinanciamiento.Text, out decimal fin) ? fin / 100 : 0;
            decimal pUti = decimal.TryParse(txtApuUtilidad.Text, out decimal uti) ? uti / 100 : 0;
            decimal pIva = decimal.TryParse(txtApuIva.Text, out decimal iva) ? iva / 100 : 0.16m;

            decimal ci = cd * pInd;
            decimal sub1 = cd + ci;
            decimal cf = sub1 * pFin;
            decimal sub2 = sub1 + cf;
            decimal cu = sub2 * pUti;
            decimal pu = sub2 + cu;
            decimal ivaM = pu * pIva;
            decimal total = pu + ivaM;

            lblSubtotalMateriales.Text = $"${subMat:0.00}";
            lblPctMateriales.Text = $"{pctMat:0.00}%";
            lblSubtotalManoObra.Text = $"${subMO:0.00}";
            lblPctManoObra.Text = $"{pctMO:0.00}%";
            lblSubtotalEquipo.Text = $"${subEq:0.00}";
            lblPctEquipo.Text = $"{pctEq:0.00}%";

            lblCostoDirecto.Text = $"${cd:0.00}";
            lblIndirectos.Text = $"${ci:0.00}";
            lblSubtotal1.Text = $"${sub1:0.00}";
            lblFinanciamiento.Text = $"${cf:0.00}";
            lblSubtotal2.Text = $"${sub2:0.00}";
            lblUtilidad.Text = $"${cu:0.00}";
            lblResTotal.Text = $"${pu:0.00}";
            lblResIva.Text = $"${ivaM:0.00}";
            lblTotalConIva.Text = $"${total:0.00}";

            lblApuPrecioUnitario.Text = $"${pu:0.00}";
        }

        // ── Generar tabla de Presupuesto ──────────────────────
        private void GenerarPresupuesto()
        {
            itemsPresupuesto.Clear();
            int no = 1;

            decimal pInd = decimal.TryParse(txtApuIndirectos.Text, out decimal ind) ? ind / 100 : 0;
            decimal pFin = decimal.TryParse(txtApuFinanciamiento.Text, out decimal fin) ? fin / 100 : 0;
            decimal pUti = decimal.TryParse(txtApuUtilidad.Text, out decimal uti) ? uti / 100 : 0;

            // Genera un item de presupuesto por cada concepto completo del APU
            // (cada fila de mano de obra representa un concepto de trabajo)
            foreach (var mo in filasManoObra)
            {
                // Precio unitario del concepto = precio MO + materiales relacionados + equipo
                // Por simplicidad: usamos el precio unitario calculado del APU
                decimal subMat = materialesSeleccionados.Sum(m => m.Subtotal);
                decimal subMO = mo.Subtotal;
                decimal subEq = filasEquipo.Sum(f => f.Subtotal);
                decimal cd = subMat + subMO + subEq;
                decimal ci = cd * pInd;
                decimal sub1 = cd + ci;
                decimal cf = sub1 * pFin;
                decimal sub2 = sub1 + cf;
                decimal cu = sub2 * pUti;
                decimal pu = sub2 + cu;

                itemsPresupuesto.Add(new ItemPresupuesto
                {
                    No = no++,
                    Concepto = mo.Material,
                    Unidad = mo.Unidad,
                    Cantidad = mo.Cantidad,
                    PU = pu
                });
            }

            // Si no hay mano de obra, genera un item general
            if (filasManoObra.Count == 0)
            {
                decimal subMat = materialesSeleccionados.Sum(m => m.Subtotal);
                decimal subEq = filasEquipo.Sum(f => f.Subtotal);
                decimal cd = subMat + subEq;
                decimal ci = cd * pInd;
                decimal sub1 = cd + ci;
                decimal cf = sub1 * pFin;
                decimal sub2 = sub1 + cf;
                decimal cu = sub2 * pUti;
                decimal pu = sub2 + cu;

                itemsPresupuesto.Add(new ItemPresupuesto
                {
                    No = 1,
                    Concepto = txtApuDescripcion.Text,
                    Unidad = txtApuUnidad.Text,
                    Cantidad = decimal.TryParse(txtApuCantidad.Text, out decimal q) ? q : 1,
                    PU = pu
                });
            }

            gridPresupuesto.ItemsSource = null;
            gridPresupuesto.ItemsSource = itemsPresupuesto;

            // Actualizar totales del presupuesto
            decimal subTotal = itemsPresupuesto.Sum(i => i.Importe);
            decimal ivaTotal = subTotal * 0.16m;
            decimal totalFin = subTotal + ivaTotal;

            lblPresupuestoSub.Text = $"${subTotal:0.00}";
            lblPresupuestoIva.Text = $"${ivaTotal:0.00}";
            lblPresupuestoTotal.Text = $"${totalFin:0.00}";
        }

        // ── Exportar PDF APU ──────────────────────────────────
        private void BtnApuPDF_Click(object sender, RoutedEventArgs e)
        {
            if (materialesSeleccionados.Count == 0 &&
                filasManoObra.Count == 0 &&
                filasEquipo.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar");
                return;
            }

            var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
            if (dialog.ShowDialog() != true) return;

            CalcularAPU();

            decimal subMat = materialesSeleccionados.Sum(m => m.Subtotal);
            decimal subMO = filasManoObra.Sum(f => f.Subtotal);
            decimal subEq = filasEquipo.Sum(f => f.Subtotal);
            decimal cd = subMat + subMO + subEq;

            decimal pInd = decimal.TryParse(txtApuIndirectos.Text, out decimal ind) ? ind / 100 : 0;
            decimal pFin = decimal.TryParse(txtApuFinanciamiento.Text, out decimal fin) ? fin / 100 : 0;
            decimal pUti = decimal.TryParse(txtApuUtilidad.Text, out decimal uti) ? uti / 100 : 0;
            decimal pIva = decimal.TryParse(txtApuIva.Text, out decimal iva) ? iva / 100 : 0.16m;

            decimal ci = cd * pInd;
            decimal sub1 = cd + ci;
            decimal cf = sub1 * pFin;
            decimal sub2 = sub1 + cf;
            decimal cu = sub2 * pUti;
            decimal pu = sub2 + cu;
            decimal ivaM = pu * pIva;
            decimal total = pu + ivaM;

            using (PdfWriter writer = new PdfWriter(dialog.FileName))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.SetMargins(20, 20, 20, 20);

                document.Add(new Paragraph("ANÁLISIS DE PRECIOS UNITARIOS")
                    .SetFontSize(16).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(
                    $"Obra: {txtApuObra.Text}   |   Lugar: {txtApuLugar.Text}   |   Fecha: {DateTime.Now:dd/MM/yyyy}")
                    .SetFontSize(9).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(
                    $"Análisis: {txtApuNumero.Text}   |   {txtApuDescripcion.Text}   |   Unidad: {txtApuUnidad.Text}   |   Cantidad: {txtApuCantidad.Text}")
                    .SetFontSize(10));
                document.Add(new Paragraph(" "));

                void SeccionHeader(string titulo)
                {
                    var t = new Table(new float[] { 700 }).UseAllAvailableWidth();
                    t.AddCell(new Cell()
                        .Add(new Paragraph(titulo).SetFontSize(10))
                        .SetBackgroundColor(new DeviceRgb(90, 127, 168))
                        .SetFontColor(ColorConstants.WHITE)
                        .SetPadding(4)
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    document.Add(t);
                }

                void TablaFilas(List<MaterialAPU> filas, string[] headers)
                {
                    float[] w = { 90, 230, 60, 80, 60, 90 };
                    var t = new Table(UnitValue.CreatePointArray(w)).UseAllAvailableWidth();
                    foreach (var h in headers)
                    {
                        t.AddHeaderCell(new Cell()
                            .Add(new Paragraph(h).SetFontSize(9))
                            .SetBackgroundColor(new DeviceRgb(200, 200, 200))
                            .SetTextAlignment(iTextTextAlignment.CENTER)
                            .SetPadding(4)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    }
                    foreach (var f in filas)
                    {
                        double pct = cd > 0 ? (double)(f.Subtotal / cd * 100) : 0;
                        t.AddCell(CreateCellSm(f.NumeroSerie ?? "", iTextTextAlignment.LEFT));
                        t.AddCell(CreateCellSm(f.Material ?? "", iTextTextAlignment.LEFT));
                        t.AddCell(CreateCellSm(f.Unidad ?? "", iTextTextAlignment.CENTER));
                        t.AddCell(CreateCellSm($"${f.PrecioUnitario:0.00}", iTextTextAlignment.RIGHT));
                        t.AddCell(CreateCellSm(f.Cantidad.ToString(), iTextTextAlignment.CENTER));
                        t.AddCell(CreateCellSm($"${f.Subtotal:0.00} {pct:0.00}%", iTextTextAlignment.RIGHT));
                    }
                    document.Add(t);
                }

                void SubtotalSeccion(string label, decimal monto)
                {
                    double pct = cd > 0 ? (double)(monto / cd * 100) : 0;
                    var t = new Table(new float[] { 500, 200 }).UseAllAvailableWidth();
                    t.AddCell(new Cell()
                        .Add(new Paragraph($"SUBTOTAL: {label}").SetFontSize(9))
                        .SetTextAlignment(iTextTextAlignment.RIGHT)
                        .SetPadding(4).SetBorder(new SolidBorder(1)));
                    t.AddCell(new Cell()
                        .Add(new Paragraph($"${monto:0.00}   {pct:0.00}%").SetFontSize(9))
                        .SetTextAlignment(iTextTextAlignment.RIGHT)
                        .SetBackgroundColor(new DeviceRgb(26, 58, 107))
                        .SetFontColor(ColorConstants.WHITE)
                        .SetPadding(4).SetBorder(new SolidBorder(1)));
                    document.Add(t);
                    document.Add(new Paragraph(" "));
                }

                SeccionHeader("MATERIALES");
                TablaFilas(materialesSeleccionados,
                    new[] { "N° Serie", "Material", "Unidad", "P. Unitario", "Cantidad", "Subtotal" });
                SubtotalSeccion("MATERIALES", subMat);

                SeccionHeader("MANO DE OBRA");
                TablaFilas(filasManoObra,
                    new[] { "Clave", "Descripción", "Unidad", "Salario", "Cantidad", "Importe" });
                SubtotalSeccion("MANO DE OBRA", subMO);

                SeccionHeader("EQUIPO Y HERRAMIENTA");
                TablaFilas(filasEquipo,
                    new[] { "Clave", "Descripción", "Unidad", "Costo/Hr", "Cantidad", "Importe" });
                SubtotalSeccion("EQUIPO Y HERRAMIENTA", subEq);

                var tRes = new Table(new float[] { 300, 100, 150 }).UseAllAvailableWidth();

                void FilaRes(string label, string pct, string valor, bool azul = false)
                {
                    var bg = azul ? new DeviceRgb(26, 58, 107) : new DeviceRgb(245, 245, 245);
                    var fg = azul ? ColorConstants.WHITE : ColorConstants.BLACK;
                    tRes.AddCell(new Cell().Add(new Paragraph(label).SetFontSize(9))
                        .SetTextAlignment(iTextTextAlignment.RIGHT)
                        .SetBackgroundColor(bg).SetFontColor(fg)
                        .SetPadding(4).SetBorder(new SolidBorder(1)));
                    tRes.AddCell(new Cell().Add(new Paragraph(pct).SetFontSize(9))
                        .SetTextAlignment(iTextTextAlignment.CENTER)
                        .SetBackgroundColor(bg).SetFontColor(fg)
                        .SetPadding(4).SetBorder(new SolidBorder(1)));
                    tRes.AddCell(new Cell().Add(new Paragraph(valor).SetFontSize(9))
                        .SetTextAlignment(iTextTextAlignment.RIGHT)
                        .SetBackgroundColor(bg).SetFontColor(fg)
                        .SetPadding(4).SetBorder(new SolidBorder(1)));
                }

                FilaRes("(CD) Costo Directo", "100.00%", $"${cd:0.00}", true);
                FilaRes("(CI) INDIRECTOS", $"{ind:0.0000}%", $"${ci:0.00}");
                FilaRes("SUBTOTAL 1", "", $"${sub1:0.00}");
                FilaRes("(CF) FINANCIAMIENTO", $"{fin:0.0000}%", $"${cf:0.00}");
                FilaRes("SUBTOTAL 2", "", $"${sub2:0.00}");
                FilaRes("(CU) UTILIDAD", $"{uti:0.0000}%", $"${cu:0.00}");
                FilaRes("PRECIO UNITARIO (CD+CI+CF+CU)", "", $"${pu:0.00}", true);
                FilaRes("IVA", $"{iva:0}%", $"${ivaM:0.00}");
                FilaRes("TOTAL CON IVA", "", $"${total:0.00}", true);

                document.Add(tRes);

                // ── PRESUPUESTO en el mismo PDF ──
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                ExportarPresupuestoPDF(document, ind, iva);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dialog.FileName,
                UseShellExecute = true
            });

            MessageBox.Show("PDF generado correctamente");
        }

        // ── Exportar Presupuesto PDF ──────────────────────────
        private void ExportarPresupuestoPDF(Document document, decimal pctInd, decimal pctIva)
        {
            GenerarPresupuesto();

            document.Add(new Paragraph(txtApuObra.Text)
                .SetFontSize(14).SetTextAlignment(iTextTextAlignment.CENTER));
            document.Add(new Paragraph($"Lugar: {txtApuLugar.Text}   |   Fecha: {DateTime.Now:dd 'de' MMMM 'del' yyyy}")
                .SetFontSize(9).SetTextAlignment(iTextTextAlignment.CENTER));
            document.Add(new Paragraph(" "));

            // Título presupuesto centrado
            var tTit = new Table(new float[] { 700 }).UseAllAvailableWidth();
            tTit.AddCell(new Cell()
                .Add(new Paragraph("PRESUPUESTO").SetFontSize(13))
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(26, 58, 107))
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(6).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
            document.Add(tTit);

            // Subtítulo obra
            var tSub = new Table(new float[] { 700 }).UseAllAvailableWidth();
            tSub.AddCell(new Cell()
                .Add(new Paragraph(txtApuDescripcion.Text.ToUpper()).SetFontSize(10))
                .SetTextAlignment(iTextTextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(180, 180, 180))
                .SetPadding(4).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
            document.Add(tSub);
            document.Add(new Paragraph(" "));

            // Tabla presupuesto
            float[] w = { 30, 250, 60, 60, 80, 80 };
            var t = new Table(UnitValue.CreatePointArray(w)).UseAllAvailableWidth();

            foreach (var h in new[] { "No.", "CONCEPTO", "UNIDAD", "CANTIDAD", "P.U.", "IMPORTE" })
            {
                t.AddHeaderCell(new Cell()
                    .Add(new Paragraph(h).SetFontSize(9))
                    .SetBackgroundColor(new DeviceRgb(26, 58, 107))
                    .SetFontColor(ColorConstants.WHITE)
                    .SetTextAlignment(iTextTextAlignment.CENTER)
                    .SetPadding(5).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
            }

            foreach (var item in itemsPresupuesto)
            {
                t.AddCell(CreateCellSm(item.No.ToString(), iTextTextAlignment.CENTER));
                t.AddCell(CreateCellSm(item.Concepto ?? "", iTextTextAlignment.LEFT));
                t.AddCell(CreateCellSm(item.Unidad ?? "", iTextTextAlignment.CENTER));
                t.AddCell(CreateCellSm(item.Cantidad.ToString("0.0000"), iTextTextAlignment.CENTER));
                t.AddCell(CreateCellSm($"${item.PU:0.00}", iTextTextAlignment.RIGHT));
                t.AddCell(CreateCellSm($"${item.Importe:0.00}", iTextTextAlignment.RIGHT));
            }

            // Filas vacías para llenar
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                    t.AddCell(CreateCellSm("", iTextTextAlignment.LEFT));
            }

            document.Add(t);
            document.Add(new Paragraph(" "));

            // Totales
            decimal subTotal = itemsPresupuesto.Sum(i => i.Importe);
            decimal ivaTotal = subTotal * (pctIva / 100);
            decimal totalFin = subTotal + ivaTotal;

            var tTot = new Table(new float[] { 480, 80, 100 }).UseAllAvailableWidth();

            void FilaTot(string label, string valor, bool gris = false)
            {
                var bg = gris ? new DeviceRgb(220, 220, 220) : new DeviceRgb(255, 255, 255);
                tTot.AddCell(new Cell().Add(new Paragraph("").SetFontSize(9))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                tTot.AddCell(new Cell()
                    .Add(new Paragraph(label).SetFontSize(10))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(bg).SetPadding(5)
                    .SetBorder(new SolidBorder(1)));
                tTot.AddCell(new Cell()
                    .Add(new Paragraph(valor).SetFontSize(10))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(bg).SetPadding(5)
                    .SetBorder(new SolidBorder(1)));
            }

            FilaTot("SUB", $"${subTotal:0.00}");
            FilaTot("IVA", $"${ivaTotal:0.00}");
            FilaTot("TOTAL", $"${totalFin:0.00}", true);

            document.Add(tTot);
            document.Add(new Paragraph(" "));

            // Condiciones
            document.Add(new Paragraph("Condiciones")
                .SetFontSize(11));
            document.Add(new Paragraph(
                "Cualquier omisión no prevista debe cotizarse de manera adicional.")
                .SetFontSize(9));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));

            // Firma
            document.Add(new Paragraph("____________________________________")
                .SetTextAlignment(iTextTextAlignment.CENTER));
            document.Add(new Paragraph("Ing. / Empresa")
                .SetTextAlignment(iTextTextAlignment.CENTER).SetFontSize(9));
        }

        // ── Exportar PDF Presupuesto independiente ────────────
        private void BtnPresupuestoPDF_Click(object sender, RoutedEventArgs e)
        {
            if (itemsPresupuesto.Count == 0)
            {
                MessageBox.Show("Primero presiona Calcular para generar el presupuesto");
                return;
            }

            var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
            if (dialog.ShowDialog() != true) return;

            decimal pInd = decimal.TryParse(txtApuIndirectos.Text, out decimal ind) ? ind / 100 : 0;
            decimal pIva = decimal.TryParse(txtApuIva.Text, out decimal iva) ? iva : 16;

            using (PdfWriter writer = new PdfWriter(dialog.FileName))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.SetMargins(20, 20, 20, 20);
                ExportarPresupuestoPDF(document, pInd, pIva);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dialog.FileName,
                UseShellExecute = true
            });

            MessageBox.Show("Presupuesto PDF generado correctamente");
        }

    } // ← cierra MainWindow

} // ← cierra namespace