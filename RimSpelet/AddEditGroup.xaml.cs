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
using System.Windows.Shapes;
using System.Data.EntityClient;

namespace RimSpelet
{
    /// <summary>
    /// Interaction logic for AddEditGroup.xaml
    /// </summary>
    public partial class AddEditGroup : Window
    {
        public AddEditGroup()
        {
            InitializeComponent();
        }

        public AddEditGroup(Rime rimeToEdit)
            : this()
        {
            textBox1.Text = rimeToEdit.RimeWord;
            isNewValue = false;
            Value = rimeToEdit;
        }

        public Rime ReturnValue
        {
            get;
            set;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Value != null)
            {
                Value.RimeWord = textBox1.Text;
                ReturnValue = Value;
            }
            else
            {
                using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
                {
                    using (DatabaseEntities context = new DatabaseEntities())
                    {
                        if (textBox1.Text != string.Empty)
                        {
                            Rime newRime = new Rime();
                            newRime.RimeWord = textBox1.Text;
                            context.AddToRime(newRime);
                            context.SaveChanges();
                            ReturnValue = newRime;
                        }
                    }
                }
            }
            Close();
        }

        private bool isNewValue { get; set; }
        private Rime Value { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
