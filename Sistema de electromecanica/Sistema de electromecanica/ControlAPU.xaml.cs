using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaCotizacionesWPF
{
    public partial class ControlAPU : UserControl
    {
        // ── Datos del control ─────────────────────────────────
        public int NumeroAPU { get; set; }

        public readonly List<MaterialAPU> MatSeleccionados = new();
        public readonly List<MaterialAPU> FilasMO = new();
        public readonly List<MaterialAPU> FilasEq = new();

        // Catálogos compartidos que se inyectan desde MainWindow
        private List<MaterialAPU> _catalogoMat = new();
        private List<MaterialAPU> _catalogoMO = new();
        private List<MaterialAPU> _catalogoEq = new();

        // Evento para notificar a MainWindow que recalcule el presupuesto
        public event Action? CambioEnDatos;

        // ── Constructor ───────────────────────────────────────
        public ControlAPU()
        {
            InitializeComponent();
        }

        public void Inicializar(
            int numero,
            List<MaterialAPU> catalogoMat,
            List<MaterialAPU> catalogoMO,
            List<MaterialAPU> catalogoEq)
        {
            NumeroAPU = numero;
            _catalogoMat = catalogoMat;
            _catalogoMO = catalogoMO;
            _catalogoEq = catalogoEq;

            lblNumeroAPU.Text = $"Actividad {numero}";

            gridCatalogoMat.ItemsSource = _catalogoMat;
            gridCatalogoMO.ItemsSource = _catalogoMO;
            gridCatalogoEq.ItemsSource = _catalogoEq;

            gridMatSeleccionados.ItemsSource = MatSeleccionados;
            gridMO.ItemsSource = FilasMO;
            gridEq.ItemsSource = FilasEq;
        }

        public void ActualizarNumero(int numero)
        {
            NumeroAPU = numero;
            lblNumeroAPU.Text = $"Actividad {numero}";
        }

        // ── Propiedades calculadas ────────────────────────────
        public string Descripcion => txtDescripcionActividad.Text;
        public string UnidadAPU => txtUnidadAPU.Text;
        public decimal CantidadAPU =>
            decimal.TryParse(txtCantidadAPU.Text, out decimal q) ? q : 1;

        public decimal PrecioUnitario
        {
            get
            {
                decimal pInd = decimal.TryParse(txtIndirectos.Text, out decimal i) ? i / 100 : 0;
                decimal pFin = decimal.TryParse(txtFinanciamiento.Text, out decimal f) ? f / 100 : 0;
                decimal pUti = decimal.TryParse(txtUtilidad.Text, out decimal u) ? u / 100 : 0;

                decimal cd = MatSeleccionados.Sum(m => m.Subtotal)
                             + FilasMO.Sum(m => m.Subtotal)
                             + FilasEq.Sum(m => m.Subtotal);
                decimal ci = cd * pInd;
                decimal sub1 = cd + ci;
                decimal cf = sub1 * pFin;
                decimal sub2 = sub1 + cf;
                decimal cu = sub2 * pUti;
                return sub2 + cu;
            }
        }

        // ── Calcular ──────────────────────────────────────────
        public void Calcular()
        {
            foreach (var f in FilasMO) f.Subtotal = f.PrecioUnitario * f.Cantidad;
            foreach (var f in FilasEq) f.Subtotal = f.PrecioUnitario * f.Cantidad;

            decimal subMat = MatSeleccionados.Sum(m => m.Subtotal);
            decimal subMO = FilasMO.Sum(f => f.Subtotal);
            decimal subEq = FilasEq.Sum(f => f.Subtotal);
            decimal cd = subMat + subMO + subEq;

            double pctMat = cd > 0 ? (double)(subMat / cd * 100) : 0;
            double pctMO = cd > 0 ? (double)(subMO / cd * 100) : 0;
            double pctEq = cd > 0 ? (double)(subEq / cd * 100) : 0;

            decimal pInd = decimal.TryParse(txtIndirectos.Text, out decimal ind) ? ind / 100 : 0;
            decimal pFin = decimal.TryParse(txtFinanciamiento.Text, out decimal fin) ? fin / 100 : 0;
            decimal pUti = decimal.TryParse(txtUtilidad.Text, out decimal uti) ? uti / 100 : 0;

            decimal ci = cd * pInd;
            decimal sub1 = cd + ci;
            decimal cf = sub1 * pFin;
            decimal sub2 = sub1 + cf;
            decimal cu = sub2 * pUti;
            decimal pu = sub2 + cu;

            lblSubMat.Text = $"${subMat:0.00}";
            lblPctMat.Text = $"{pctMat:0.00}%";
            lblSubMO.Text = $"${subMO:0.00}";
            lblPctMO.Text = $"{pctMO:0.00}%";
            lblSubEq.Text = $"${subEq:0.00}";
            lblPctEq.Text = $"{pctEq:0.00}%";

            lblCD.Text = $"${cd:0.00}";
            lblCI.Text = $"${ci:0.00}";
            lblSub1.Text = $"${sub1:0.00}";
            lblCF.Text = $"${cf:0.00}";
            lblSub2.Text = $"${sub2:0.00}";
            lblCU.Text = $"${cu:0.00}";
            lblPU.Text = $"${pu:0.00}";

            lblPrecioUnitarioAPU.Text = $"${pu:0.00}";

            CambioEnDatos?.Invoke();
        }

        // ── Helpers de grid ───────────────────────────────────
        private void RefrescarMat()
        {
            gridMatSeleccionados.ItemsSource = null;
            gridMatSeleccionados.ItemsSource = MatSeleccionados;
        }
        private void RefrescarMO()
        {
            gridMO.ItemsSource = null;
            gridMO.ItemsSource = FilasMO;
        }
        private void RefrescarEq()
        {
            gridEq.ItemsSource = null;
            gridEq.ItemsSource = FilasEq;
        }

        // ── Eventos Materiales ────────────────────────────────
        private void BtnAgregarMat_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoMat.SelectedItem is not MaterialAPU sel) return;
            if (!int.TryParse(txtCantidadMat.Text, out int cant) || cant <= 0) return;

            var ex = MatSeleccionados.FirstOrDefault(m => m.NumeroSerie == sel.NumeroSerie);
            if (ex != null)
            {
                ex.Cantidad += cant;
                ex.Subtotal = ex.PrecioUnitario * ex.Cantidad;
            }
            else
            {
                MatSeleccionados.Add(new MaterialAPU
                {
                    NumeroSerie = sel.NumeroSerie,
                    Material = sel.Material,
                    Unidad = sel.Unidad,
                    PrecioUnitario = sel.PrecioUnitario,
                    Cantidad = cant,
                    Subtotal = sel.PrecioUnitario * cant
                });
            }
            RefrescarMat();
            Calcular();
            txtCantidadMat.Text = "1";
        }

        private void BtnQuitarMat_Click(object sender, RoutedEventArgs e)
        {
            if (gridMatSeleccionados.SelectedItem is not MaterialAPU sel) return;
            MatSeleccionados.Remove(sel);
            RefrescarMat();
            Calcular();
        }

        // ── Eventos Mano de Obra ──────────────────────────────
        private void BtnAgregarMO_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoMO.SelectedItem is not MaterialAPU sel) return;
            if (!int.TryParse(txtCantidadMO.Text, out int cant) || cant <= 0) return;

            var ex = FilasMO.FirstOrDefault(m => m.NumeroSerie == sel.NumeroSerie);
            if (ex != null)
            {
                ex.Cantidad += cant;
                ex.Subtotal = ex.PrecioUnitario * ex.Cantidad;
            }
            else
            {
                FilasMO.Add(new MaterialAPU
                {
                    NumeroSerie = sel.NumeroSerie,
                    Material = sel.Material,
                    Unidad = sel.Unidad,
                    PrecioUnitario = sel.PrecioUnitario,
                    Cantidad = cant,
                    Subtotal = sel.PrecioUnitario * cant
                });
            }
            RefrescarMO();
            Calcular();
            txtCantidadMO.Text = "1";
        }

        private void BtnQuitarMO_Click(object sender, RoutedEventArgs e)
        {
            if (gridMO.SelectedItem is not MaterialAPU sel) return;
            FilasMO.Remove(sel);
            RefrescarMO();
            Calcular();
        }

        // ── Eventos Equipo ────────────────────────────────────
        private void BtnAgregarEq_Click(object sender, RoutedEventArgs e)
        {
            if (gridCatalogoEq.SelectedItem is not MaterialAPU sel) return;
            if (!int.TryParse(txtCantidadEq.Text, out int cant) || cant <= 0) return;

            var ex = FilasEq.FirstOrDefault(m => m.NumeroSerie == sel.NumeroSerie);
            if (ex != null)
            {
                ex.Cantidad += cant;
                ex.Subtotal = ex.PrecioUnitario * ex.Cantidad;
            }
            else
            {
                FilasEq.Add(new MaterialAPU
                {
                    NumeroSerie = sel.NumeroSerie,
                    Material = sel.Material,
                    Unidad = sel.Unidad,
                    PrecioUnitario = sel.PrecioUnitario,
                    Cantidad = cant,
                    Subtotal = sel.PrecioUnitario * cant
                });
            }
            RefrescarEq();
            Calcular();
            txtCantidadEq.Text = "1";
        }

        private void BtnQuitarEq_Click(object sender, RoutedEventArgs e)
        {
            if (gridEq.SelectedItem is not MaterialAPU sel) return;
            FilasEq.Remove(sel);
            RefrescarEq();
            Calcular();
        }

        // ── CellEditEnding ────────────────────────────────────
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(Calcular),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        // ── Factores ─────────────────────────────────────────
        private void TxtFactor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblPU != null) Calcular();
        }

        // ── Eliminar este APU ─────────────────────────────────
        private void BtnEliminarAPU_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"¿Eliminar Actividad {NumeroAPU}?",
                "Confirmar", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                MarcarParaEliminar = true;
                CambioEnDatos?.Invoke();
            }
        }

        public bool MarcarParaEliminar { get; private set; }

        private void BtnEliminarAPU_ClickReal(object sender, RoutedEventArgs e)
        {
            MarcarParaEliminar = true;
            CambioEnDatos?.Invoke();
        }
    }
}