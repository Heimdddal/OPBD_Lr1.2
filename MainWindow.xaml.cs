using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;


namespace OPBD_Lr1._2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string dbFileName;
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;
        private DateTime forOut;
        private string dateFormat = @"dd-MM-yyyy";
        public MainWindow()
        {
            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();
            dbFileName = "carShowroom.sqlite";
            
            InitializeComponent();
        }
        
        private DataSet bdQuerry(string tableName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                string sqlQuery;
                sqlQuery = $"select * from {tableName}";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, m_dbConn);
                adapter.Fill(dataSet);
                return dataSet;
            }
            catch (SQLiteException exception)
            {
                MessageBox.Show("Error:" + exception.Message);
                return dataSet;
            }
            catch (System.ArgumentException ex)
            {
                MessageBox.Show("Error: " + ex);
                return dataSet;
            }
        } 

        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile(dbFileName);
            }

            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;

                m_sqlCmd.CommandText = "PRAGMA foreign_keys = ON;";
                m_sqlCmd.ExecuteNonQuery();

                m_sqlCmd.CommandText =
                    "create table if not exists Positions(PositionId integer primary key autoincrement," +
                    "PositionName text not null," +
                    "Salary real not null," +
                    "Responsibilities text," +
                    "Requirements text," +
                    "check ( Salary > 0 ));" +
                    "create table if not exists Employees(" +
                    "employeeId integer primary key autoincrement," +
                    "SecondName text not null," +
                    "FirstName text not null," +
                    "Patronymic text," +
                    "BirthDate text(11) not null," +
                    "Gender text(1) not null," +
                    "Address text not null," +
                    "PhoneNumber integer," +
                    "PassportSeries integer not null," +
                    "PassportNumber integer not null," +
                    "PositionId integer," +
                    "FOREIGN KEY (PositionId) references Positions(PositionId));" +
                    "create table if not exists Manufacturers(" +
                    "ManufacturerId integer primary key autoincrement," +
                    "ManufacturerName text not null," +
                    "ManufacturerCountry text not null," +
                    "ManufacturerAddress text not null," +
                    "EmployeeId integer," +
                    "FOREIGN KEY (EmployeeId) references Employees(employeeId));";
                m_sqlCmd.ExecuteNonQuery();

                ConnectionStatus.Foreground = Brushes.Chartreuse;
                ConnectionStatus.Text = "CONNECTED";
            }
            catch (SQLiteException exception)
            {
                ConnectionStatus.Foreground = Brushes.Red;
                ConnectionStatus.Text = "DISCONNECTED";
                MessageBox.Show("Error:" + exception.Message);
            }
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(dbFileName))
            {
                MessageBox.Show("Please, create DB and blank table(Push\"Create\"button)");
                ConnectionStatus.Foreground = Brushes.Red;
                ConnectionStatus.Text = "DISCONNECTED";
                return;
            }

            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;
            
                ConnectionStatus.Foreground = Brushes.Chartreuse;
                ConnectionStatus.Text = "CONNECTED";
                
                using (var pragmaCommand = new SQLiteCommand("PRAGMA foreign_keys;", m_dbConn))
                {
                    var result = Convert.ToInt32(pragmaCommand.ExecuteScalar());

                    if (result == 1)
                    {
                        MessageBox.Show("Проверка внешних ключей включена.");
                    }
                    else
                    {
                        m_sqlCmd.CommandText = "PRAGMA foreign_keys = ON;";
                        m_sqlCmd.ExecuteNonQuery();
                            
                        MessageBox.Show("Проверка внешних ключей выключена.");
                    }
                }
            }
            catch (SQLiteException exception)
            {
                ConnectionStatus.Foreground = Brushes.Red;
                ConnectionStatus.Text = "DISCONNECTED";
                MessageBox.Show("Error:" + exception.Message);
            }
        }


        private void ReadAll_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                PositionsDataGrid.ItemsSource = bdQuerry("Positions").Tables[0].DefaultView;

                EmployeesDataGrid.ItemsSource = bdQuerry("Employees").Tables[0].DefaultView;

                ManufacturersDataGrid.ItemsSource = bdQuerry("Manufacturers").Tables[0].DefaultView;
            }
            catch (System.IndexOutOfRangeException exception)
            {
                MessageBox.Show("Для начала подключите базу данных");
            }
            catch(Exception exception)
            {
                MessageBox.Show("Error: " + exception);
            }
            
        }

        private void AddManufacturer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(@Manufacturer.Text) || string.IsNullOrWhiteSpace(@ManufacturerCountry.Text)||
                    string.IsNullOrWhiteSpace(@ManufacturerAddress.Text))
                {
                    MessageBox.Show("Обязательные поля не могут быть пустыми");
                    return;
                }
                
                if (m_dbConn.State != ConnectionState.Open)
                {
                    MessageBox.Show("Open connection");
                    return;
                }

                try
                {
                    m_sqlCmd.CommandText =
                        $"insert into Manufacturers('ManufacturerName', 'ManufacturerCountry', 'ManufacturerAddress', 'EmployeeId')" +
                        $"values('{@Manufacturer.Text}','{@ManufacturerCountry.Text}','{@ManufacturerAddress.Text}','{@Convert.ToInt32(ManufacturerEmployer.Text)}');";

                    m_sqlCmd.ExecuteNonQuery();
                    
                    ManufacturersDataGrid.ItemsSource = bdQuerry("Manufacturers").Tables[0].DefaultView;
                }
                catch (SQLiteException exception)
                {
                    MessageBox.Show("Error:" + exception.Message);
                }
            }
            catch (System.FormatException exception)
            {
                MessageBox.Show("Код сотрудника указан неверно");
            }
        }

        private void AddPosition_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@PositionName.Text))
                {
                    MessageBox.Show("Обязательные поля не могут быть пустыми");
                    return;
                }
                
                if (m_dbConn.State != ConnectionState.Open)
                {
                    MessageBox.Show("Open connection");
                    return;
                }

                try
                {
                    m_sqlCmd.CommandText =
                        $"insert into Positions('PositionName', 'Salary', 'Responsibilities', 'Requirements')" +
                        $"values('{@PositionName.Text}','{@Convert.ToInt32(Salary.Text)}','{@Responsibilities.Text}','{@Requirements.Text}');";

                    m_sqlCmd.ExecuteNonQuery();
                    
                    PositionsDataGrid.ItemsSource = bdQuerry("Positions").Tables[0].DefaultView;
                }
                catch (SQLiteException exception)
                {
                    MessageBox.Show("Error:" + exception.Message);
                }
            }
            catch (System.FormatException exception)
            {
                MessageBox.Show("Информация о окладе введена неправильно");
            }
        }

        private void AddEmployee_OnClick(object sender, RoutedEventArgs e)
        {   
            try
            {
                if (string.IsNullOrWhiteSpace(@SName.Text) || string.IsNullOrWhiteSpace(@FName.Text) ||
                    string.IsNullOrWhiteSpace(@BirthDate.Text) || string.IsNullOrWhiteSpace(@Address.Text))
                {
                    MessageBox.Show("Обязательные поля не могут быть пустыми");
                    return;
                }

                if (PassportSeries.Text.Length != 4)
                {
                    MessageBox.Show("Серия пасспорта введена неправильно");
                    return;
                }

                if (PassportNumber.Text.Length !=6)
                {
                    MessageBox.Show("Номер пасспорта введен неправильно");
                    return;
                }


                if (!DateTime.TryParseExact(@BirthDate.Text, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    MessageBox.Show("Дата рождения введена неправильно");
                    return;
                } //TODO - добаввить проверку даты на правильность введения с помощью regex
                
                //TODO - добавить проверку даты на правильность введения относительно текущей даты(ограничение на максимальный и минимальный возраст)
                
                if (m_dbConn.State != ConnectionState.Open)
                {
                    MessageBox.Show("Open connection");
                    return;
                }

                try
                {
                    m_sqlCmd.CommandText =
                        $"insert into Employees('SecondName', 'FirstName', 'Patronymic', 'BirthDate', 'Gender', 'Address', 'PhoneNumber', 'PassportSeries', 'PassportNumber', 'PositionId')" +
                        $"values('{@SName.Text}','{@FName.Text}','{@Patronymic.Text}','{@BirthDate.Text}', " +
                        $"'{Gender.Text}', '{@Address.Text}', '{@PhoneNumber.Text}', " +
                        $"'{Convert.ToInt32(PassportSeries.Text)}', '{Convert.ToInt32(PassportNumber.Text)}', '{Convert.ToInt32(EmployeePosition.Text)}');";

                    m_sqlCmd.ExecuteNonQuery();
                    
                    EmployeesDataGrid.ItemsSource = bdQuerry("Employees").Tables[0].DefaultView;
                }
                catch (SQLiteException exception)
                {
                    if (exception.ErrorCode == Convert.ToInt32(SQLiteErrorCode.Constraint_ForeignKey))
                    {
                        MessageBox.Show("Ошибка внешнего ключа: " + exception.Message);
                    }
                    else
                    {
                        MessageBox.Show("Error:" + exception.Message);
                    }
                }
            }
            catch (System.FormatException exception)
            {
                MessageBox.Show("Серия паспорта и/или номер паспорта введены неправильно");
            }
        }
    }
}