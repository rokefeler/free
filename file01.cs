using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using prjGStionPRO.PRESENTACION.Recursos.Util;
using prjGStionPRO.LOGICA;
using System.ComponentModel;
using prjGStionPRO.PRESENTACION.Utilitarios;
using prjGStionPRO.PRESENTACION.Reportes_Gstion.Clases;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using prjGStionPRO.Presentacion.Variables;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using prjGStionPRO.PRESENTACION.Recursos.Comandos;
using System.Windows.Automation.Peers;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace prjGStionPRO.PRESENTACION.Modulos.Produccion
{
    /// <summary>
    /// Interaction logic for frmOrdenServicio.xaml
    /// </summary>
    public partial class FrmOrdenRecetaProducto : UserControl
    {
        readonly MetodosComunes _busqueda = new MetodosComunes();
        private ClRecetaProduccion _objectoRecetaProduccion;
        public ICollectionView DetalleProductosView { get; private set; }
        public ICollectionView DetallePersonalView { get; private set; }
        public List<ClPersonal> PersonalList { get => _personalList; set => _personalList = value; }
        public List<ClOcupaciones> OcupacionList { get => _ocupacionList; set => _ocupacionList = value; }

        //private ClCargo  _objetoCargo;
        private ClPersonal _objetoPersonal;
        private ClOcupaciones _objetoOcupacion;
        private ClProducto _objectoProducto;
        private ClProducto _objectoProductoVenta;
        private List<ClPersonal> _personalList;
        private List<ClOcupaciones> _ocupacionList;

        public delegate Point GetPosition(IInputElement element);
        int rowIndex = -1;

        public FrmOrdenRecetaProducto()
        {
            InitializeComponent();
            this.DataContext = this;
            ConfiguracionScreen();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UserControlAutomationPeer(this);
        }
        
        // setting del formulario
        private void ConfiguracionScreen()
        {
            var vm = layoutRecetaProduccion.DataContext as ClRecetaProduccion;
            ModulosVariables.MetodoActual = "RecetaProduccion";
            ModulosVariables.ObjectoActual = vm;
            ModulosVariables.FormularioActual = this;
            _objectoRecetaProduccion = vm;

            CargarLista();

            if (ModulosVariables.TipoFormulario == 3) // cunado se abre el formulario como producto de popup, cuidado puede que no funcione bien
            {
                _objectoRecetaProduccion = (ClRecetaProduccion)ModulosVariables.ObjectoOrigen;

                if (ModulosVariables.FormularioMetodoBusquedaOrigen != null)
                {
                    _busqueda.BuscarClase(ModulosVariables.FormularioActual, "ResultadoBusqueda_" + ModulosVariables.FormularioMetodoBusquedaOrigen);
                    ModulosVariables.FormularioMetodoBusquedaOrigen = null;
                }
                else
                {
                    _busqueda.BuscarClase(ModulosVariables.FormularioActual, "ResultadoBusqueda_" + ModulosVariables.MetodoBusqueda);
                }
                AsignarDetalleProduccion();
                CompletarObjecto();
                Habilitar_Controles();
            }
            else // por defecto
            {
                if (ModulosVariables.ObjectoBusqueda != null)
                {
                    _objectoRecetaProduccion = (ClRecetaProduccion)ModulosVariables.ObjectoBusqueda;

                    ModulosVariables.vgNoOfErrorsOnScreen = -4;

                    _objectoRecetaProduccion.cargarRecetaProduccionProductos(_objectoRecetaProduccion.Codigo);
                    _objectoRecetaProduccion.cargarRecetaProduccionPersona(_objectoRecetaProduccion.Codigo);

                    _objectoRecetaProduccion.DetalleProductos = _objectoRecetaProduccion.DetalleProductos;
                    _objectoRecetaProduccion.DetalleCuadrilla = _objectoRecetaProduccion.DetalleCuadrilla;

                    AsignarDetalleProduccion();
                    CompletarObjecto();
                    Habilitar_Controles();
                }
                else
                {
                    _objectoRecetaProduccion = vm;
                    ModulosVariables.vgNoOfErrorsOnScreen = -1;
                }

                Desahabilitar_Controles();
            }

        }

        #region "metodos y funciones - formulario"

        // llena combobox de tipos de personal o cargos
        private void CargarLista()
        {
            var negocio = new NegocioPersonal();
            var items = negocio.GetPersonal();
            //rokefeler PersonalList = items;
            _personalList = items;
            var negocioOcupacion = new NegocioOcupaciones();
            var itemsOcupacion = negocioOcupacion.GetOcupaciones();
            //rokefeler OcupacionList = itemsOcupacion;
            _ocupacionList = itemsOcupacion;
            if (items != null)
            {

                cbPersonal.ItemsSource = _personalList; //rokefeler items;
                cbPersonal.DisplayMemberPath = "Nombre";
                cbPersonal.SelectedValuePath = "Interno";

                cbOcupacion.ItemsSource = _ocupacionList; //rokefeler itemsOcupacion;
                cbOcupacion.DisplayMemberPath = "Nombre";
                cbOcupacion.SelectedValuePath = "Interno";
                
            }
            else
            {
                _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "Hubo un error al cargar la lista de tipo de personal");
            }
        }

        // si el objeto principal contiene detalles de productos y personal son presentados en las grilla
        private void AsignarDetalleProduccion()
        {
            if (_objectoRecetaProduccion != null)
            {
                dgdetalleProductos.ItemsSource = null;
                dgdetalleProductos.Items.Clear();
                dgdetallePersonal.ItemsSource = null;
                dgdetallePersonal.Items.Clear();

                if (_objectoRecetaProduccion.DetalleProductos.Count > 0)
                {
                    DetalleProductosView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleProductos);
                    dgdetalleProductos.ItemsSource = DetalleProductosView;
                }
                if (_objectoRecetaProduccion.DetalleCuadrilla.Count > 0)
                {
                    DetallePersonalView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleCuadrilla);
                    dgdetallePersonal.ItemsSource = DetallePersonalView;
                }
            }
        }

        // conteo de controles del formulario a validar 
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                ModulosVariables.validacionScreen += 1;
            }
            else
            {
                ModulosVariables.validacionScreen -= 1;
            }
        }

        // el objeto principal receta es asigando como contexto del formulario
        private void CompletarObjecto()
        {
            layoutRecetaProduccion.DataContext = _objectoRecetaProduccion;
            txtproductoVenta.Text = string.Empty;
            txtCodigoProducto.Text = string.Empty;
            if (_objectoRecetaProduccion.CodigoProducto > 0)
            {
                txtproductoVenta.Text = _objectoRecetaProduccion.Producto.prod_descripcion;
                txtCodigoProducto.Text = _objectoRecetaProduccion.Producto.prod_codigo.ToString(CultureInfo.InvariantCulture);
            }
            VerificarCheckEstado();
            ModulosVariables.ObjectoActual = _objectoRecetaProduccion;
        }

        private void Desahabilitar_Controles()
        {
            DeshabilitarControlesProductos(false);
            DeshabilitarControlesPersonal(false);
            ActivarControles(false);
        }

        private void ActivarControles(bool activar)
        {
            dtfechaactual.IsEnabled = activar;
            cbPersonal.IsEnabled = activar;
            cbOcupacion.IsEnabled = activar;
            dgdetalleProductos.IsEnabled = activar;
            dgdetallePersonal.IsEnabled = activar;
            btnbuscarProductoventa.IsEnabled = activar;
            btnbuscar_producto.IsEnabled = activar;
            dUDPorcentajeCostosIndirectos.IsEnabled = activar;
            dUDPorcentajeUtilidad.IsEnabled = activar;
            dUDTipoCambioDolar.IsEnabled = activar;
            CkApruebaRecta.IsEnabled = activar;
        }

        private void Habilitar_Controles()
        {
            ActivarControles(true);
            dtfechaactual.SelectedDate = DateTime.Now; // fecha actual de sistema
            _objectoRecetaProduccion.IgvActual = ModulosVariables.ObjectoConfiguracionIGV.igv; // igv actua
            _objectoRecetaProduccion.TipoCambioDolar = ModulosVariables.ObjectoTipoCambio.monto; // cambio actual 

            dgdetalleProductos.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(productsDataGrid_PreviewMouseLeftButtonDown);
            dgdetalleProductos.Drop += new DragEventHandler(productsDataGrid_Drop);
        }

        // se asigna la formulario el objeto producto resultado dela busqueda
        public void ResultadoBusqueda_Producto()
        {
            switch (ModulosVariables.ParametroBusquedaPopup)
            {
                case 1:

                    _objectoProductoVenta = (ClProducto)ModulosVariables.ObjectoBusqueda;
                    if (_objectoProductoVenta != null)
                    {
                        txtproductoVenta.Text = _objectoProductoVenta.prod_descripcion;
                        _objectoRecetaProduccion.CodigoProducto = _objectoProductoVenta.prod_codigo;
                        _objectoRecetaProduccion.CodigoAsignado = _objectoProductoVenta.prod_codigoGenerado;
                        var negocio = new NegocioRecetaProducto();
                        List<ClRecetaProduccion> listaValidacion = negocio.BuscarRecetaProduccion(_objectoProductoVenta.prod_codigo.ToString(CultureInfo.InvariantCulture), "2");
                        if (listaValidacion.Count == 1)
                        {
                            _objectoRecetaProduccion = listaValidacion[0];
                            _objectoRecetaProduccion.cargarRecetaProduccionProductos(_objectoRecetaProduccion.Codigo);
                            _objectoRecetaProduccion.cargarRecetaProduccionPersona(_objectoRecetaProduccion.Codigo);

                            _objectoRecetaProduccion.DetalleProductos = _objectoRecetaProduccion.DetalleProductos;
                            _objectoRecetaProduccion.DetalleCuadrilla = _objectoRecetaProduccion.DetalleCuadrilla;

                            AsignarDetalleProduccion();

                            layoutRecetaProduccion.DataContext = _objectoRecetaProduccion;
                            ModulosVariables.ObjectoActual = _objectoRecetaProduccion;
                            //ModulosVariables.vgNoOfErrorsOnScreen = -2;
                        }
                    }
                    break;
                case 2:
                    var producto = ModulosVariables.ObjectoBusqueda as ClProducto;

                    if (producto != null)
                    {
                        _objectoProducto = producto;

                        if (_objectoProducto != null)
                        {
                            txtproducto.Text = _objectoProducto.prod_descripcion;
                            numeric_Precio.Value = decimal.Round(_objectoProducto.ProdPrecioKardex, 2);
                            numeric_total.Value = MetodoGeneral.RedondearDecimales((numeric_Precio.Value.Value * Convert.ToDecimal(numericCantidad.Value)), 2);
                            DeshabilitarControlesProductos(true);
                        }
                        else
                        {
                            DeshabilitarControlesProductos(false);
                        }
                    }
                    break;
            }
        }

        private void DeshabilitarControlesProductos(bool activar)
        {
            numericCantidad.IsEnabled = activar;
            numeric_Precio.IsEnabled = activar;
            btnAgregarProducto.IsEnabled = activar;
            txtObservacionesProducto.IsEnabled = activar;
        }

        private void Sumas()
        {
            if (_objectoProducto != null)
            {
                numeric_total.Value = numeric_Precio.Value * numericCantidad.Value;
            }
        }

        private void SumasPersonalProducccion()
        {
            //personalNumeric_total.Value = decimal.Zero;
            if (cbPersonal.SelectedItem != null && personalNumeric_total!=null)
            {
                personalNumeric_total.Value = personalNumeric_Precio.Value * personalNumericCantidad.Value;
            }
        }

        // controles incrustados en grilla  dgdetalleProductos
        private void LimpiardetallesProductos()
        {
            numeric_Precio.Value = Convert.ToDecimal(0.1);
            numeric_total.Value = 0;
            numericCantidad.Value = 1;
            txtproducto.Clear();
            txtObservacionesProducto.Clear();
            DeshabilitarControlesProductos(false);
        }

        // controles incrustados en grilla  dgdetallePersonal
        private void LimpiardetallesPersonal()
        {
            personalNumeric_Precio.Value = Convert.ToDecimal(0.1);
            personalNumeric_total.Value = 0;
            personalNumericCantidad.Value = 1;
            personalNumericCantidad.IsEnabled = false;
            personalNumeric_Precio.IsEnabled = false;
            btnAgregarPersonal.IsEnabled = false;
        }

        private void DeshabilitarControlesPersonal(bool activar)
        {
            personalNumericCantidad.IsEnabled = activar;
            personalNumeric_Precio.IsEnabled = activar;
            btnAgregarPersonal.IsEnabled = activar;
            ObservacionesPersonal.IsEnabled = activar;
        }

        // segun el estado de la receta (1 = pendiente) se activa o deasctiva la casilla
        private void VerificarCheckEstado()
        {
            if (_objectoRecetaProduccion != null)
            {
                if (_objectoRecetaProduccion.EstadoReceta == 1)
                {
                    CkApruebaRecta.IsChecked = true;
                }
                else
                {
                    CkApruebaRecta.IsChecked = false;
                }
            }
        }

        #endregion

        #region "eventos - controles genericos"

        public void popupdAtajo()
        {
            ModulosVariables.ObjectoImpresion.TipoAtajo = 3;
            var resultado = MessageBox.Show("Desea Visualizar la Receta", "Aviso!", MessageBoxButton.OKCancel);
            if (resultado == MessageBoxResult.OK)
            {
                cargar_ExportarDocumento();
            }
            else
            {
                ModulosVariables.ObjectoBusqueda = null;
                try
                {
                    _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "Navegacion_Libre", "/Modulos/Produccion/frmBusquedaRecetaProducto.xaml");
                }
                catch (Exception ex)
                {
                    _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", ex.Message);
                }
            }
        }

        public void cargar_ImprimirDocumento() // no hace nada
        {
            //cargar_ExportarDocumento();
        }

        // consulta y generacion del reporte por codigo asignado manualmente
        public void cargar_ExportarDocumento()
        {
            IReporteOrdenRecetaProduccion generarReportes = new reporteOrdenRecetaProduccion();
            if (_objectoRecetaProduccion != null)
            {
                generarReportes.GenerarReporteOrdenProduccion(_objectoRecetaProduccion.CodigoAsignado);
            }
        }

        public void NuevoRecetaProduccion()
        {
            Habilitar_Controles();
            var negocioReceta = new NegocioRecetaProducto();
            var resultado = negocioReceta.ultimaRecetaProduccion(); // consulta ultima receta ingresada
            if (resultado == null)
            {
                _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "Occurrió un error al consultar los porcentajes de costos y utilidad");
            }
            else
            {
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = resultado.PorcentajeCostoIndirecto; // mismo costo indirecto de ultima receta
                _objectoRecetaProduccion.PorcentajeUtilidad = resultado.PorcentajeUtilidad; // misma utilidad de la ultima receta
            }

            //if (_objectoRecetaProduccion == null || resultado == null) return;
            //if (_objectoRecetaProduccion.Correlativo != 0) return;
        }

        public void LimpiarDatos()
        {
            if (_objectoRecetaProduccion != null) // respetar este orden cuando se necesite limpiar el formulario
            {
                layoutRecetaProduccion.DataContext = new ClRecetaProduccion(); // 1
                ModulosVariables.ObjectoActual = layoutRecetaProduccion.DataContext as ClRecetaProduccion; // 2
                _objectoRecetaProduccion = (ClRecetaProduccion)ModulosVariables.ObjectoActual; // 3
            }

            Desahabilitar_Controles();
            txtproductoVenta.Clear();
            LimpiardetallesProductos();
            LimpiardetallesPersonal();

            if (DetalleProductosView != null)
            {
                dgdetalleProductos.ItemsSource = null;
                DetalleProductosView = null;
            }
            else
            {
                dgdetalleProductos.Items.Clear();
            }

            if (DetallePersonalView != null)
            {
                dgdetallePersonal.ItemsSource = null;
                DetallePersonalView = null;
            }
            else
            {
                dgdetallePersonal.Items.Clear();
            }
        }

        #endregion

        #region "eventos - controles"

        private void CkApruebaRecta_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_objectoRecetaProduccion != null) _objectoRecetaProduccion.EstadoReceta = 1; // estado aprobado
        }

        private void CkApruebaRecta_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_objectoRecetaProduccion != null) _objectoRecetaProduccion.EstadoReceta = 0;
        }

        private void UpDownCantidadBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = (ClDetalleReceta)dgdetalleProductos.SelectedItem;

                var resultado = sender as DecimalUpDown;

                if (resultado == null) return;

                if (item != null)
                {
                    if (item.CodigoProductoInsumo > 0)
                    {
                        if (resultado.Value == 0 || resultado.Value == null)
                        {
                            resultado.Value = Convert.ToDecimal("0.01");
                        }
                    }
                    else
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }
                else
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }

                _objectoRecetaProduccion.MontoMateriaPrima = _objectoRecetaProduccion.DetalleProductos.Where(d => d.EsProductoInsumo = true).Sum(d => d.PrecioTotal);
                // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
            catch
            {
                ;
            }
        }

        private void UpDownPrecioBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = (ClDetalleReceta)dgdetalleProductos.SelectedItem;

                var resultado = sender as DecimalUpDown;
                if (resultado == null) return;

                if (item != null)
                {
                    if (item.CodigoProductoInsumo > 0)
                    {
                        if (resultado.Value == 0 || resultado.Value == null)
                        {
                            resultado.Value = Convert.ToDecimal("0.01");
                        }
                    }
                }
                else
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }

                _objectoRecetaProduccion.MontoMateriaPrima = _objectoRecetaProduccion.DetalleProductos.Where(d => d.EsProductoInsumo = true).Sum(d => d.PrecioTotal);
                // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
            catch
            {
                ;
            }
        }

        private void UpDownCantidadPersonalBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = (ClDetalleReceta)dgdetalleProductos.SelectedItem;

                var resultado = sender as DecimalUpDown;

                if (resultado == null) return;

                if (item != null)
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }
                else
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }

                _objectoRecetaProduccion.MontoManoObra = _objectoRecetaProduccion.DetalleCuadrilla.Sum(d => d.PrecioTotal);
                // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
            catch
            {
                ;
            }
        }

        private void UpDownPrecioPersonalBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var item = (ClDetalleReceta)dgdetalleProductos.SelectedItem;

                var resultado = sender as DecimalUpDown;
                if (resultado == null) return;

                if (item != null)
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }
                else
                {
                    if (resultado.Value == 0 || resultado.Value == null)
                    {
                        resultado.Value = Convert.ToDecimal("0.01");
                    }
                }

                _objectoRecetaProduccion.MontoManoObra = _objectoRecetaProduccion.DetalleCuadrilla.Sum(d => d.PrecioTotal);
                // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
            catch
            {
                ;
            }
        }

        private void btnbuscarProductoventa_Click(object sender, RoutedEventArgs e)
        {
            ModulosVariables.ParametroBusquedaPopup = 1;
            _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "busquedasSimple", "/Modulos/Productos/frmBusquedaProductosMinimo.xaml");
        }

        private void cbCargoPersonal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_objetoCargo = cbCargoPersonal.SelectedItem as ClCargo;
            try
            {
                _objetoPersonal = cbPersonal.SelectedItem as ClPersonal;
                _objetoOcupacion = cbOcupacion.SelectedItem as ClOcupaciones;


                /*if (_objetoCargo != null)
                {
                    DeshabilitarControlesPersonal(true);
                    personalNumeric_Precio.Value = _objetoCargo.PagoHora;
                    SumasPersonalProducccion();
                }*/
                if (_objetoPersonal != null)
                {
                    DeshabilitarControlesPersonal(true);
                    personalNumeric_Precio.Value = _objetoPersonal.PagoHora;
                    SumasPersonalProducccion();
                }
                else
                {
                    LimpiardetallesPersonal();
                    DeshabilitarControlesPersonal(false);
                }
            }
            catch
            {
                ;
            }
        }

        private void btnEliminarItemPersonal_Click(object sender, RoutedEventArgs e)
        {
            if (_objectoRecetaProduccion != null)
            {
                if (dgdetallePersonal != null)
                {
                    try
                    {
                        _objectoRecetaProduccion.MontoSubTotal -= ((ClDetalleReceta)dgdetallePersonal.SelectedItem).PrecioTotal;
                    }
                    catch {;}
                    try
                    {
                        _objectoRecetaProduccion.DetalleCuadrilla.Remove((ClDetalleReceta)dgdetallePersonal.SelectedItem);
                    }
                    catch {;}
                }
                _objectoRecetaProduccion.MontoManoObra = _objectoRecetaProduccion.DetalleCuadrilla.Sum(d => d.PrecioTotal);
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
                DetallePersonalView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleCuadrilla);
                DetallePersonalView.Refresh();
                //rokefeler bucle _objectoRecetaProduccion.DetalleCuadrilla = _objectoRecetaProduccion.DetalleCuadrilla;
            }
        }

        private void btnEliminarProductoItem_Click(object sender, RoutedEventArgs e)
        {
            if (_objectoRecetaProduccion != null)
            {
                _objectoRecetaProduccion.MontoSubTotal -= ((ClDetalleReceta)dgdetalleProductos.SelectedItem).PrecioTotal;
                _objectoRecetaProduccion.DetalleProductos.Remove((ClDetalleReceta)dgdetalleProductos.SelectedItem);
                _objectoRecetaProduccion.MontoMateriaPrima = _objectoRecetaProduccion.DetalleProductos.Sum(d => d.PrecioTotal);
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
                DetalleProductosView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleProductos);
                DetalleProductosView.Refresh();
                //rokefeler bucle _objectoRecetaProduccion.DetalleProductos = _objectoRecetaProduccion.DetalleProductos;
            }
        }

        private void personalNumericCantidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SumasPersonalProducccion();
        }

        private void personalNumeric_Precio_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SumasPersonalProducccion();
        }

        private void btnAgregarPersonal_Click(object sender, RoutedEventArgs e)
        {
            var preguntarMensaje = "Esta Seguro que desea Agregar el Personal Seleccionado";
            //08May23
            if (_objetoOcupacion == null)
            {
                MessageBox.Show("Seleccione una ocupación", "Aviso", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                return;
            }
            var confirma = MessageBox.Show(preguntarMensaje, "Esta Seguro?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirma == MessageBoxResult.No)
            {
                return;
            }

            if (Convert.ToInt32(personalNumericCantidad.Value) > 0)
            {
                ClDetalleReceta registroEncontradoTemp = (from element in _objectoRecetaProduccion.DetalleCuadrilla
                                                              //where element.CodigoCargoPersonal == _objetoPersonal.Correlativo
                                                          where element.CodigoPersonal == _objetoPersonal.Codigo
                                                          select element).FirstOrDefault();

                decimal cantidadReal;
                decimal cantidadTotal;

                if (registroEncontradoTemp != null)
                {
                    _objectoRecetaProduccion.MontoSubTotal -= registroEncontradoTemp.PrecioTotal;
                    //_objectoRecetaProduccion.DetalleCuadrilla.Remove(registroEncontradoTemp);
                    cantidadReal = registroEncontradoTemp.Cantidad + (int)personalNumericCantidad.Value;
                    cantidadTotal = decimal.Round((cantidadReal * (decimal)personalNumeric_Precio.Value), 2);
                }
                else
                {
                    try
                    {
                        cantidadReal = (decimal)personalNumericCantidad.Value;
                    }
                    catch
                    {
                        cantidadReal = decimal.Zero;
                    }
                    try
                    {
                        cantidadTotal = (decimal)personalNumeric_total.Value;
                    }
                    catch
                    {
                        cantidadTotal = decimal.Zero;
                    }
                }

                var data = new ClDetalleReceta
                {
                    EsProductoInsumo = false,
                    Personal = _objetoPersonal,
                    Ocupacion = _objetoOcupacion,

                    DescripcionCargoPersonal = _objetoOcupacion.Nombre,
                    //CodigoCargoPersonal =_objetoPersonal.Codigo,
                    CodigoPersonal = _objetoPersonal.Codigo,
                    CodigoOcupacion = _objetoOcupacion.Codigo,

                    Cantidad = cantidadReal,
                    PrecioUnidad = (decimal)personalNumeric_Precio.Value,
                    PrecioTotal = cantidadTotal,
                    Observaciones = ObservacionesPersonal.Text
                };

                if (dgdetallePersonal.Items.Count < 100 && !_objectoRecetaProduccion.DetalleCuadrilla.Contains(data))
                {
                    _objectoRecetaProduccion.DetalleCuadrilla.Add(data);
                    _objectoRecetaProduccion.MontoSubTotal += decimal.Round((decimal)personalNumeric_total.Value, 2);
                    //duplicado _objectoRecetaProduccion.DetalleCuadrilla = _objectoRecetaProduccion.DetalleCuadrilla;
                    _objectoRecetaProduccion.MontoManoObra = _objectoRecetaProduccion.DetalleCuadrilla.Sum(d => d.PrecioTotal);
                    //duplicado _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                    //duplicado _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                    //duplicado _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;

                    //dgdetallePersonal.IsEnabled = false;
                    //dgdetallePersonal.ItemsSource = null; //10may23
                    //dgdetallePersonal.Items.Clear();
                    //DetallePersonalView.DeferRefresh();
                    //BindingOperations.DisableCollectionSynchronization(DetallePersonalView);//.Disable(dgdetallePersonal, dgdetallePersonal.ItemsSource);
                    DetallePersonalView = null;
                    DetallePersonalView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleCuadrilla);
                    DetallePersonalView.Refresh();
                    dgdetallePersonal.ItemsSource = DetallePersonalView;

                    /* limpiar datos ingresados */
                    cbPersonal.IsEnabled = false;
                    cbOcupacion.IsEnabled = false;
                    cbPersonal.SelectedIndex = -1;
                    cbOcupacion.SelectedIndex = -1;
                    personalNumericCantidad.Value = decimal.Zero;
                    personalNumeric_Precio.Value = decimal.Zero;
                    personalNumeric_total.Value = decimal.Zero;
                    ObservacionesPersonal.Text = string.Empty;
                    
                    //dgdetallePersonal.IsEnabled = true;

                    cbPersonal.IsEnabled = true;
                    cbOcupacion.IsEnabled = true;
                    LimpiardetallesPersonal();
                    //BindingOperations.EnableCollectionSynchronization(DetallePersonalView);//.Disable(dgdetallePersonal, dgdetallePersonal.ItemsSource);
                }
                else
                {
                    _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "El límite para agregar personal es de 100 items");
                }
            }
            else
            {
                _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "Debe indicar un costo por hora válido");
            }
        }

        private void btnbuscar_producto_Click(object sender, RoutedEventArgs e)
        {
            ModulosVariables.ParametroBusquedaPopup = 2;
            _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "busquedasSimple", "/Modulos/Productos/frmBusquedaProductosMinimo.xaml");
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var preguntarMensaje = "Esta Seguro que desea Agregar el Producto Seleccionado";
            var confirma = MessageBox.Show(preguntarMensaje, "Esta Seguro?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirma == MessageBoxResult.No)
            {
                return;
            }

            if (Convert.ToDecimal(numericCantidad.Value) > 0)
            {
                ClDetalleReceta productoEncontradoTemp = (from element in _objectoRecetaProduccion.DetalleProductos
                                                          where element.CodigoProductoInsumo == _objectoProducto.prod_codigo
                                                          select element).FirstOrDefault();

                decimal cantidadTotal;
                decimal cantidadReal;

                if (productoEncontradoTemp != null)
                {
                    _objectoRecetaProduccion.MontoSubTotal -= productoEncontradoTemp.PrecioTotal;
                    _objectoRecetaProduccion.DetalleProductos.Remove(productoEncontradoTemp);
                    cantidadReal = productoEncontradoTemp.Cantidad + (decimal)numericCantidad.Value;
                    cantidadTotal = decimal.Round((cantidadReal * (decimal)numeric_Precio.Value), 2);
                }
                else
                {
                    try
                    {
                        cantidadReal = (decimal)numericCantidad.Value;
                    }
                    catch
                    {
                        cantidadReal = decimal.Zero;
                    }
                    try
                    {
                        cantidadTotal = (decimal)numeric_total.Value;
                    }
                    catch
                    {
                        cantidadTotal = decimal.Zero;
                    }
                }


                var data = new ClDetalleReceta
                {
                    EsProductoInsumo = true,
                    Producto = _objectoProducto,
                    CodigoProductoInsumo = _objectoProducto.prod_codigo,
                    descripcion = txtproducto.Text,
                    Cantidad = cantidadReal,
                    PrecioUnidad = (decimal)numeric_Precio.Value,
                    PrecioTotal = cantidadTotal,
                    Observaciones = txtObservacionesProducto.Text
                };

                if (dgdetalleProductos.Items.Count < 500 && !_objectoRecetaProduccion.DetalleProductos.Contains(data))
                {
                    _objectoRecetaProduccion.DetalleProductos.Add(data);
                    _objectoRecetaProduccion.MontoSubTotal += decimal.Round((decimal)numeric_total.Value, 2);
                    _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                    _objectoRecetaProduccion.DetalleProductos = _objectoRecetaProduccion.DetalleProductos;

                    dgdetalleProductos.ItemsSource = null;
                    dgdetalleProductos.Items.Clear();
                    DetalleProductosView = CollectionViewSource.GetDefaultView(_objectoRecetaProduccion.DetalleProductos);
                    dgdetalleProductos.ItemsSource = DetalleProductosView;
                    _objectoRecetaProduccion.MontoMateriaPrima = _objectoRecetaProduccion.DetalleProductos.Sum(d => d.PrecioTotal);
                    _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                    _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                    _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;

                    LimpiardetallesProductos();
                }
                else
                {
                    _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "El límite para agregar productos es de 100 items");
                }
            }
            else
            {
                _busqueda.BuscarClase(ModulosVariables.ObjectoContenedor, "mensaje", "Debe indicar un precio valido");
            }

        }

        private void numericCantidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Sumas();
        }

        private void numeric_Precio_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Sumas();
        }

        private void dUDPorcentajeCostosIndirectos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_objectoRecetaProduccion != null)
            {    // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
        }

        private void dUDPorcentajeUtilidad_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_objectoRecetaProduccion != null)
            {    // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
        }

        private void dUDTipoCambioDolar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_objectoRecetaProduccion != null)
            {    // provoca el evento en el objeto de contexto o principal que actualiza los montos ante un cambio de los mismos
                _objectoRecetaProduccion.PorcentajeCostoIndirecto = _objectoRecetaProduccion.PorcentajeCostoIndirecto;
                _objectoRecetaProduccion.PorcentajeUtilidad = _objectoRecetaProduccion.PorcentajeUtilidad;
                _objectoRecetaProduccion.TipoCambioDolar = _objectoRecetaProduccion.TipoCambioDolar;
            }
        }

        void productsDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (rowIndex < 0)
                return;
            int index = this.GetCurrentRowIndex(e.GetPosition);
            if (index < 0)
                return;
            if (index == rowIndex)
                return;
            if (index == dgdetalleProductos.Items.Count - 1)
            {
                MessageBox.Show("Este índice no se puede arrastrar");
                return;
            }

            List<ClDetalleReceta> productCollection = DetalleProductosView.SourceCollection as List<ClDetalleReceta>;

            ClDetalleReceta changedProduct = productCollection[rowIndex];
            productCollection.RemoveAt(rowIndex);
            productCollection.Insert(index, changedProduct);
            dgdetalleProductos.Items.Refresh();
        }

        void productsDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;
            dgdetalleProductos.SelectedIndex = rowIndex;
            var selectedEmp = dgdetalleProductos.Items[rowIndex];
            if (selectedEmp == null)
                return;
            DragDropEffects dragdropeffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(dgdetalleProductos, selectedEmp, dragdropeffects)
                                != DragDropEffects.None)
            {
                dgdetalleProductos.SelectedItem = selectedEmp;
            }
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            if (theTarget == null)
            {
                return false;
            }

            Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point point = position((IInputElement)theTarget);
            return rect.Contains(point);
        }

        private Microsoft.Windows.Controls.DataGridRow GetRowItem(int index)
        {
            if (dgdetalleProductos.ItemContainerGenerator.Status
                    != GeneratorStatus.ContainersGenerated)
                return null;
            Microsoft.Windows.Controls.DataGridRow element = dgdetalleProductos.ItemContainerGenerator.ContainerFromIndex(index) as Microsoft.Windows.Controls.DataGridRow;
            return element;
        }

        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < dgdetalleProductos.Items.Count; i++)
            {
                Microsoft.Windows.Controls.DataGridRow itm = GetRowItem(i);
                if (GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }
        #endregion


    }
}
