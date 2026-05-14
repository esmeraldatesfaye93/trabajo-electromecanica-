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
        private readonly List<MaterialAPU> materialesSeleccionados = new();
        private readonly List<MaterialAPU> filasManoObra = new();
        private readonly List<MaterialAPU> filasEquipo = new();

        public MainWindow()
        {
            InitializeComponent();
            CargarDatos();
            InicializarAPU();
        }

        // ── Carga de datos ───────────────────────────────────
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

        // ── Helpers ──────────────────────────────────────────
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

                document.Add(new Paragraph("COTIZACIÓN")
                    .SetFontSize(22)
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

                string folio = "COT-" + new Random().Next(1000, 9999);
                document.Add(new Paragraph($"Folio: {folio}"));
                document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
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
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetPadding(8));
                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${totalSnapshot:0.00}").SetFontSize(14))
                    .SetTextAlignment(iTextTextAlignment.RIGHT)
                    .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                    .SetPadding(8));
                document.Add(totalTable);

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

            gridCatalogoMateriales.ItemsSource = catalogoMateriales;
            gridMaterialesSeleccionados.ItemsSource = materialesSeleccionados;
            gridManoObra.ItemsSource = filasManoObra;
            gridEquipo.ItemsSource = filasEquipo;
        }

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

        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(CalcularAPU),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void BtnCalcularAPU_Click(object sender, RoutedEventArgs e)
        {
            CalcularAPU();
        }

        private void TxtFactor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblResTotal != null)
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

            // Recalcular antes de exportar
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

                // Encabezado
                document.Add(new Paragraph("ANÁLISIS DE PRECIOS UNITARIOS")
                    .SetFontSize(16)
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(
                    $"Obra: {txtApuObra.Text}   |   Lugar: {txtApuLugar.Text}   |   Fecha: {DateTime.Now:dd/MM/yyyy}")
                    .SetFontSize(9)
                    .SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new Paragraph(" "));

                // Concepto
                document.Add(new Paragraph(
                    $"Análisis: {txtApuNumero.Text}   |   {txtApuDescripcion.Text}   |   Unidad: {txtApuUnidad.Text}   |   Cantidad: {txtApuCantidad.Text}")
                    .SetFontSize(10));
                document.Add(new Paragraph(" "));

                // Helper encabezado sección
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

                // Helper tabla de filas
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
                        t.AddCell(CreateCell(f.NumeroSerie ?? "", iTextTextAlignment.LEFT));
                        t.AddCell(CreateCell(f.Material ?? "", iTextTextAlignment.LEFT));
                        t.AddCell(CreateCell(f.Unidad ?? "", iTextTextAlignment.CENTER));
                        t.AddCell(CreateCell($"${f.PrecioUnitario:0.00}", iTextTextAlignment.RIGHT));
                        t.AddCell(CreateCell(f.Cantidad.ToString(), iTextTextAlignment.CENTER));
                        t.AddCell(CreateCell($"${f.Subtotal:0.00} {pct:0.00}%", iTextTextAlignment.RIGHT));
                    }
                    document.Add(t);
                }

                // Helper subtotal sección
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

                // ── MATERIALES ──
                SeccionHeader("MATERIALES");
                TablaFilas(materialesSeleccionados,
                    new[] { "N° Serie", "Material", "Unidad", "P. Unitario", "Cantidad", "Subtotal" });
                SubtotalSeccion("MATERIALES", subMat);

                // ── MANO DE OBRA ──
                SeccionHeader("MANO DE OBRA");
                TablaFilas(filasManoObra,
                    new[] { "Clave", "Descripción", "Unidad", "Salario", "Cantidad", "Importe" });
                SubtotalSeccion("MANO DE OBRA", subMO);

                // ── EQUIPO Y HERRAMIENTA ──
                SeccionHeader("EQUIPO Y HERRAMIENTA");
                TablaFilas(filasEquipo,
                    new[] { "Clave", "Descripción", "Unidad", "Costo/Hr", "Cantidad", "Importe" });
                SubtotalSeccion("EQUIPO Y HERRAMIENTA", subEq);

                // ── RESUMEN ──
                var tRes = new Table(new float[] { 300, 100, 150 }).UseAllAvailableWidth();

                void FilaRes(string label, string pct, string valor, bool azul = false)
                {
                    var bg = azul
                        ? new DeviceRgb(26, 58, 107)
                        : new DeviceRgb(245, 245, 245);
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
                FilaRes($"IVA", $"{iva:0}%", $"${ivaM:0.00}");
                FilaRes("TOTAL CON IVA", "", $"${total:0.00}", true);

                document.Add(tRes);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dialog.FileName,
                UseShellExecute = true
            });

            MessageBox.Show("PDF del APU generado correctamente");
        }

    } // ← cierra MainWindow

} // ← cierra namespace