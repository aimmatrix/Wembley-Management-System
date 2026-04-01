using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace ManageUser
{
    public class ManageUsersForm : Form
    {
        private UserManagementSystem _userSystem;
        private DataGridView dgvUsers;
        private ComboBox cmbRoles;
        private Button btnUpdateRole;

        public ManageUsersForm(UserManagementSystem userSystem)
        {
            _userSystem = userSystem;
            InitializeComponents();
            LoadUsers();
        }

        private void InitializeComponents()
        {
            this.Text = "Manage User Roles";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            dgvUsers = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(540, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            Label lblRole = new Label { Text = "Select New Role:", Location = new Point(20, 290), Size = new Size(100, 20) };
            
            cmbRoles = new ComboBox { Location = new Point(130, 288), Size = new Size(150, 25) };
            // This is used to show the correct role options including both Verified and Unverified Business
            cmbRoles.Items.AddRange(new string[] { "Verified_Business", "Unverified_Business" });

            btnUpdateRole = new Button { Text = "Update Role", Location = new Point(300, 285), Size = new Size(120, 30), BackColor = Color.LightGreen };
            btnUpdateRole.Click += BtnUpdateRole_Click;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRoles);
            this.Controls.Add(btnUpdateRole);
        }

        private void LoadUsers()
        {
            var userNodes = _userSystem.GetAllUsers();

            // This is used to only show business accounts (Verified and Unverified) since the admin does not need to manage clients
            var businessUsers = userNodes
                .Where(n => n != null && (n.User.UserRole == "Verified_Business" || n.User.UserRole == "Unverified_Business"))
                .Select(n => n.User)
                .ToArray();

            dgvUsers.DataSource = businessUsers;
        }

        private void BtnUpdateRole_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0 && cmbRoles.SelectedItem != null)
            {
                var selectedUser = (User)dgvUsers.SelectedRows[0].DataBoundItem;
                selectedUser.UserRole = cmbRoles.SelectedItem.ToString();

                _userSystem.UpdateUser(selectedUser.UserID, selectedUser);
                LoadUsers(); // Refresh Grid
                MessageBox.Show($"Role for {selectedUser.Username} updated to {selectedUser.UserRole}.");
            }
            else
            {
                MessageBox.Show("Please select a user and a role to apply.");
            }
        }
    }
}