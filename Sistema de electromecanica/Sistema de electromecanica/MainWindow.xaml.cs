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
    // ── Modelos ──────────────────────────────────────────────
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

    public class ItemPresupuesto
    {
        public int No { get; set; }
        public string? Concepto { get; set; }
        public string? Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PU { get; set; }
        public decimal Importe => PU * Cantidad;
    }

    // ── Ventana principal ────────────────────────────────────
    public partial class MainWindow : Window
    {
        // Cotización
        private readonly List<Producto> productos = new();
        private readonly List<Proveedor> proveedores = new();
        private readonly List<Precio> precios = new();
        private readonly List<Unidad> unidades = new();
        private readonly List<ItemCotizacion> items = new();
        private decimal total = 0;

        // Catálogos compartidos para todos los APUs
        private readonly List<MaterialAPU> catalogoMat = new();
        private readonly List<MaterialAPU> catalogoMO = new();
        private readonly List<MaterialAPU> catalogoEq = new();

        // Lista de controles APU activos
        private readonly List<ControlAPU> listaAPUs = new();
        private int contadorAPU = 0;

        // Presupuesto
        private readonly List<ItemPresupuesto> itemsPresupuesto = new();

        public MainWindow()
        {
            InitializeComponent();
            CargarDatos();
            CargarCatalogosAPU();
            // Agrega el primer APU automáticamente
            AgregarNuevoAPU();
        }

        // ── Catálogos APU ─────────────────────────────────────
        private void CargarCatalogosAPU()
        {
            catalogoMat.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "CAB-001", Material = "Cable THW calibre 12",           PrecioUnitario = 18,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "CAB-002", Material = "Cable THW calibre 10",           PrecioUnitario = 28,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "CAB-003", Material = "Cable de cobre desnudo",         PrecioUnitario = 45,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "TUB-001", Material = "Tubería conduit 3/4\"",          PrecioUnitario = 65,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "TUB-002", Material = "Tubería conduit 1\"",            PrecioUnitario = 95,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "PRO-001", Material = "Interruptor termomagnético 1P",  PrecioUnitario = 180,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "PRO-002", Material = "Interruptor termomagnético 2P",  PrecioUnitario = 320,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "PRO-003", Material = "Contactor 25A",                  PrecioUnitario = 650,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MOT-001", Material = "Motor trifásico 1HP",            PrecioUnitario = 2800, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MOT-002", Material = "Motor trifásico 3HP",            PrecioUnitario = 4500, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "TAB-001", Material = "Tablero eléctrico 12 circuitos", PrecioUnitario = 1200, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "SEN-001", Material = "Sensor de temperatura PT100",    PrecioUnitario = 380,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "LUM-001", Material = "Luminaria LED 100W industrial",  PrecioUnitario = 850,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "LUM-002", Material = "Arbotante 7.5 mts",              PrecioUnitario = 3200, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MAT-001", Material = "Cinta aislante 3M",              PrecioUnitario = 35,   Unidad = "Rollo" },
                new MaterialAPU { NumeroSerie = "MAT-002", Material = "Tornillos industriales",         PrecioUnitario = 80,   Unidad = "Caja"  },
                new MaterialAPU { NumeroSerie = "MAT-003", Material = "Guantes dieléctricos",           PrecioUnitario = 180,  Unidad = "Par"   }
            });

            catalogoMO.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "MO-001", Material = "Instalación eléctrica general",        PrecioUnitario = 500,  Unidad = "Lote"  },
                new MaterialAPU { NumeroSerie = "MO-002", Material = "Desinstalación de arbotante 7.5 mts",  PrecioUnitario = 500,  Unidad = "Lote"  },
                new MaterialAPU { NumeroSerie = "MO-003", Material = "Desinstalación de luminaria",          PrecioUnitario = 400,  Unidad = "Lote"  },
                new MaterialAPU { NumeroSerie = "MO-004", Material = "Instalación de luminaria LED",         PrecioUnitario = 350,  Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MO-005", Material = "Conexión de tablero eléctrico",        PrecioUnitario = 800,  Unidad = "Lote"  },
                new MaterialAPU { NumeroSerie = "MO-006", Material = "Tendido de cable",                     PrecioUnitario = 15,   Unidad = "Metro" },
                new MaterialAPU { NumeroSerie = "MO-007", Material = "Instalación de motor eléctrico",       PrecioUnitario = 1200, Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MO-008", Material = "Mantenimiento preventivo motor",       PrecioUnitario = 600,  Unidad = "Hora"  },
                new MaterialAPU { NumeroSerie = "MO-009", Material = "Instalación de conector bimetálico",   PrecioUnitario = 21,   Unidad = "Pieza" },
                new MaterialAPU { NumeroSerie = "MO-010", Material = "Suministro e instalación de luminaria",PrecioUnitario = 468,  Unidad = "Pieza" }
            });

            catalogoEq.AddRange(new[]
            {
                new MaterialAPU { NumeroSerie = "EQ-001", Material = "Grúa con canastilla",         PrecioUnitario = 970,  Unidad = "Hr"  },
                new MaterialAPU { NumeroSerie = "EQ-002", Material = "Andamio tubular",             PrecioUnitario = 180,  Unidad = "Día" },
                new MaterialAPU { NumeroSerie = "EQ-003", Material = "Taladro industrial",          PrecioUnitario = 120,  Unidad = "Hr"  },
                new MaterialAPU { NumeroSerie = "EQ-004", Material = "Multímetro digital",          PrecioUnitario = 50,   Unidad = "Día" },
                new MaterialAPU { NumeroSerie = "EQ-005", Material = "Escalera dieléctrica 6 mts",  PrecioUnitario = 80,   Unidad = "Día" },
                new MaterialAPU { NumeroSerie = "EQ-006", Material = "Generador eléctrico",         PrecioUnitario = 450,  Unidad = "Hr"  },
                new MaterialAPU { NumeroSerie = "EQ-007", Material = "Camión de servicio",          PrecioUnitario = 600,  Unidad = "Día" }
            });
        }

        // ── Agregar nuevo APU ─────────────────────────────────
        private void AgregarNuevoAPU()
        {
            contadorAPU++;
            var ctrl = new ControlAPU();
            ctrl.Inicializar(contadorAPU, catalogoMat, catalogoMO, catalogoEq);
            ctrl.CambioEnDatos += OnAPUCambio;
            listaAPUs.Add(ctrl);
            panelAPUs.Children.Add(ctrl);
        }

        private void OnAPUCambio()
        {
            // Eliminar APUs marcados para borrar
            var paraEliminar = listaAPUs
                .Where(a => a.MarcarParaEliminar)
                .ToList();

            foreach (var a in paraEliminar)
            {
                listaAPUs.Remove(a);
                panelAPUs.Children.Remove(a);
            }

            // Renumerar
            for (int i = 0; i < listaAPUs.Count; i++)
                listaAPUs[i].ActualizarNumero(i + 1);

            ActualizarPresupuesto();
        }

        private void BtnNuevoAPU_Click(object sender, RoutedEventArgs e)
        {
            AgregarNuevoAPU();
        }

        private void BtnActualizarPresupuesto_Click(object sender, RoutedEventArgs e)
        {
            ActualizarPresupuesto();
        }

        // ── Presupuesto consolidado ───────────────────────────
        private void ActualizarPresupuesto()
        {
            itemsPresupuesto.Clear();
            int no = 1;

            decimal pIva = decimal.TryParse(txtIvaGlobal.Text, out decimal iv) ? iv / 100 : 0.16m;

            foreach (var apu in listaAPUs)
            {
                apu.Calcular();

                // Una fila por cada concepto de MO del APU
                if (apu.FilasMO.Count > 0)
                {
                    foreach (var mo in apu.FilasMO)
                    {
                        itemsPresupuesto.Add(new ItemPresupuesto
                        {
                            No = no++,
                            Concepto = $"{apu.Descripcion}",
                            Unidad = apu.UnidadAPU,
                            Cantidad = apu.CantidadAPU,
                            PU = apu.PrecioUnitario
                        });
                    }
                }
                else
                {
                    // Si no tiene MO, agrega una fila general del APU
                    itemsPresupuesto.Add(new ItemPresupuesto
                    {
                        No = no++,
                        Concepto = apu.Descripcion,
                        Unidad = apu.UnidadAPU,
                        Cantidad = apu.CantidadAPU,
                        PU = apu.PrecioUnitario
                    });
                }
            }

            gridPresupuesto.ItemsSource = null;
            gridPresupuesto.ItemsSource = itemsPresupuesto;

            decimal subTotal = itemsPresupuesto.Sum(i => i.Importe);
            decimal ivaTotal = subTotal * pIva;
            decimal totalFin = subTotal + ivaTotal;

            lblPresupuestoSub.Text = $"${subTotal:0.00}";
            lblPresupuestoIva.Text = $"${ivaTotal:0.00}";
            lblPresupuestoTotal.Text = $"${totalFin:0.00}";
            lblIvaLabel.Text = $"IVA ({pIva * 100:0}%):";
        }

        // ── Exportar PDF Presupuesto ──────────────────────────
        private void BtnPresupuestoPDF_Click(object sender, RoutedEventArgs e)
        {
            ActualizarPresupuesto();

            if (itemsPresupuesto.Count == 0)
            {
                MessageBox.Show("No hay datos. Agrega actividades APU primero.");
                return;
            }

            var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
            if (dialog.ShowDialog() != true) return;

            decimal pIva = decimal.TryParse(txtIvaGlobal.Text, out decimal iv) ? iv / 100 : 0.16m;
            decimal subTotal = itemsPresupuesto.Sum(i => i.Importe);
            decimal ivaTotal = subTotal * pIva;
            decimal totalFin = subTotal + ivaTotal;

            using (PdfWriter writer = new PdfWriter(dialog.FileName))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.SetMargins(20, 20, 20, 20);

                // Encabezado
                document.Add(new Paragraph(txtApuObra.Text)
                    .SetFontSize(16).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph($"Lugar: {txtApuLugar.Text}   |   Fecha: {DateTime.Now:dd 'de' MMMM 'del' yyyy}")
                    .SetFontSize(9).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

                // Título
                var tTit = new Table(new float[] { 700 }).UseAllAvailableWidth();
                tTit.AddCell(new Cell()
                    .Add(new Paragraph("PRESUPUESTO").SetFontSize(13))
                    .SetTextAlignment(iTextTextAlignment.CENTER)
                    .SetBackgroundColor(new DeviceRgb(26, 58, 107))
                    .SetFontColor(ColorConstants.WHITE)
                    .SetPadding(6)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                document.Add(tTit);
                document.Add(new Paragraph(" "));

                // Tabla presupuesto
                float[] w = { 30, 280, 60, 70, 90, 100 };
                var t = new Table(UnitValue.CreatePointArray(w)).UseAllAvailableWidth();

                foreach (var h in new[] { "No.", "CONCEPTO", "UNIDAD", "CANTIDAD", "P.U.", "IMPORTE" })
                {
                    t.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFontSize(9))
                        .SetBackgroundColor(new DeviceRgb(26, 58, 107))
                        .SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(iTextTextAlignment.CENTER)
                        .SetPadding(5)
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
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

                document.Add(t);
                document.Add(new Paragraph(" "));

                // Totales
                var tTot = new Table(new float[] { 460, 90, 110 }).UseAllAvailableWidth();

                void FilaTot(string label, string valor, bool gris = false)
                {
                    var bg = gris
                        ? new DeviceRgb(210, 210, 210)
                        : new DeviceRgb(245, 245, 245);

                    tTot.AddCell(
                        new Cell()
                            .Add(new Paragraph("").SetFontSize(9))
                            .SetBorder(new SolidBorder(1))
                    );

                    tTot.AddCell(
                        new Cell()
                            .Add(new Paragraph(label).SetFontSize(10))
                            .SetTextAlignment(iTextTextAlignment.RIGHT)
                            .SetBackgroundColor(bg)
                            .SetPadding(5)
                            .SetBorder(new SolidBorder(1))
                    );

                    tTot.AddCell(
                        new Cell()
                            .Add(new Paragraph(valor).SetFontSize(10))
                            .SetTextAlignment(iTextTextAlignment.RIGHT)
                            .SetBackgroundColor(bg)
                            .SetPadding(5)
                            .SetBorder(new SolidBorder(1))
                    );
                }

                FilaTot("SUB", $"${subTotal:0.00}");
                FilaTot($"IVA ({pIva * 100:0}%)", $"${ivaTotal:0.00}");
                FilaTot("TOTAL", $"${totalFin:0.00}", true);

                document.Add(tTot);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Condiciones y firma
                document.Add(new Paragraph("Condiciones").SetFontSize(11));
                document.Add(new Paragraph(
                    "Cualquier omisión no prevista debe cotizarse de manera adicional.")
                    .SetFontSize(9));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("____________________________________")
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph("Ing. / Empresa")
                    .SetTextAlignment(iTextTextAlignment.CENTER).SetFontSize(9));
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dialog.FileName,
                UseShellExecute = true
            });

            MessageBox.Show("Presupuesto PDF generado correctamente");
        }

        // ── Helpers PDF ───────────────────────────────────────
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

        // ══════════════════════════════════════════════════════
        //              COTIZACIÓN
        // ══════════════════════════════════════════════════════

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

        private void RefrescarGrid()
        {
            grid.ItemsSource = null;
            grid.ItemsSource = items;
            lblTotal.Text = $"Total: ${total:0.00}";
        }

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

                document.Add(new Paragraph("EMPRESA INDUSTRIAL MX")
                    .SetFontSize(20).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("COTIZACIÓN")
                    .SetFontSize(18).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

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

                var totalTable = new Table(new float[] { 400, 100 });
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph("TOTAL:").SetFontSize(12))
                    .SetTextAlignment(iTextTextAlignment.RIGHT).SetPadding(8));
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${totalSnapshot:0.00}").SetFontSize(14))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230)).SetPadding(8));
                document.Add(totalTable);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Gracias por su confianza")
                    .SetFontSize(9).SetTextAlignment(iTextTextAlignment.CENTER)
                    .SetFontColor(new DeviceRgb(128, 128, 128)));
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ruta,
                UseShellExecute = true
            });

            MessageBox.Show("PDF generado correctamente");
        }

    } // ← cierra MainWindow
} // ← cierra namespace