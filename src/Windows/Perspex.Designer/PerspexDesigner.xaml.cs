﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Perspex.Designer.AppHost;
using Perspex.Designer.Comm;

namespace Perspex.Designer
{
    /// <summary>
    /// Interaction logic for PerpexDesigner.xaml
    /// </summary>
    public partial class PerspexDesigner
    {
        public static readonly DependencyProperty TargetExeProperty = DependencyProperty.Register(
            "TargetExe", typeof (string), typeof (PerspexDesigner), new FrameworkPropertyMetadata(TargetExeChanged));

        private static void TargetExeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PerspexDesigner) d).OnTargetExeChanged();
        }
        public string TargetExe
        {
            get { return (string) GetValue(TargetExeProperty); }
            set { SetValue(TargetExeProperty, value); }
        }

        public static readonly DependencyProperty XamlProperty = DependencyProperty.Register(
            "Xaml", typeof (string), typeof (PerspexDesigner), new FrameworkPropertyMetadata(XamlChanged));

        private static void XamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PerspexDesigner) d).OnXamlChanged();
        }

        public string Xaml
        {
            get { return (string) GetValue(XamlProperty); }
            set { SetValue(XamlProperty, value); }
        }

        private readonly ProcessHost _host = new ProcessHost();
        

        public PerspexDesigner()
        {
            InitializeComponent();
            BindingOperations.SetBinding(State, TextBox.TextProperty,
                new Binding(nameof(ProcessHost.State)) {Source = _host, Mode = BindingMode.OneWay});

            _host.PropertyChanged += _host_PropertyChanged;
        }

        private void _host_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProcessHost.WindowHandle))
            {
                if (NativeContainer.Content != null)
                {
                    ((WindowHost)((WindowsFormsHost)NativeContainer.Content).Child).Dispose();
                    NativeContainer.Content = null;
                }
                if (_host.WindowHandle != IntPtr.Zero)
                {
                    var host = new WindowHost();
                    host.SetWindow(_host.WindowHandle);
                    NativeContainer.Content = new WindowsFormsHost() {Child = host};
                }
            }
        }

        public void KillProcess()
        {
            _host.Kill();
        }

        private void OnTargetExeChanged()
        {
            if(string.IsNullOrEmpty(Xaml))
                return;
            _host.Start(TargetExe, Xaml);
        }

        private void OnXamlChanged()
        {
            if (!_host.IsAlive && TargetExe != null)
                _host.Start(TargetExe, Xaml);
            else
                _host.UpdateXaml(Xaml);
        }

    }
}
