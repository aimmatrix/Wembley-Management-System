using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WembleyManagementSystem;

namespace AdminUser
{
    public class AdminBusinessForm : Form
    {
        private EventManagementSystem _eventSystem;
        private UserManagementSystem _userSystem;
        private User _currentUser;

        private DataGridView dgvEvents;
        private TextBox txtEventName;
        private DateTimePicker dtpEventDate;
        private ComboBox cmbEventType;
        private NumericUpDown numPrice;
        private Button btnAddEvent;
        private Button btnDeleteEvent;
        private Button btnEditEvent;
        private Button btnSaveEdit;
        private Button btnManageUsers;

        // This is used to track which event is currently being edited
        private int _editingEventID = -1;

        public AdminBusinessForm(EventManagementSystem eventSystem, UserManagementSystem userSystem, User currentUser)
        {
            _eventSystem = eventSystem;
            _userSystem = userSystem;
            _currentUser = currentUser;
            InitializeComponents();
            LoadEvents();
        }

        private void InitializeComponents()
        {
            // This is used to set the dashboard title based on the user role
            this.Text = _currentUser.UserRole == "Admin" ? "Admin Dashboard" : "Business Dashboard";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Input Fields
            Label lblName = new Label { Text = "Event Name:", Location = new Point(20, 20), Size = new Size(80, 20) };
            txtEventName = new TextBox { Location = new Point(100, 20), Size = new Size(150, 25) };

            Label lblDate = new Label { Text = "Date:", Location = new Point(270, 20), Size = new Size(40, 20) };
            dtpEventDate = new DateTimePicker { Location = new Point(320, 20), Size = new Size(150, 25) };

            Label lblType = new Label { Text = "Type:", Location = new Point(20, 60), Size = new Size(80, 20) };
            cmbEventType = new ComboBox { Location = new Point(100, 60), Size = new Size(150, 25) };
            cmbEventType.Items.AddRange(new string[] { "Football", "Concert", "Comedy", "Other" });

            Label lblPrice = new Label { Text = "Price (£):", Location = new Point(270, 60), Size = new Size(60, 20) };
            numPrice = new NumericUpDown { Location = new Point(330, 60), Size = new Size(140, 25), Maximum = 1000 };

            // Buttons
            btnAddEvent = new Button { Text = "Add Event", Location = new Point(500, 20), Size = new Size(100, 30), BackColor = Color.LightGreen };
            btnAddEvent.Click += BtnAddEvent_Click;

            btnDeleteEvent = new Button { Text = "Delete Selected", Location = new Point(500, 60), Size = new Size(100, 30), BackColor = Color.LightCoral };
            btnDeleteEvent.Click += BtnDeleteEvent_Click;

            // This is used to load the selected event's details into the input fields for editing
            btnEditEvent = new Button { Text = "Edit Selected", Location = new Point(610, 20), Size = new Size(100, 30), BackColor = Color.LightYellow };
            btnEditEvent.Click += BtnEditEvent_Click;

            // This is used to save the changes made to the event currently being edited
            btnSaveEdit = new Button { Text = "Save Edit", Location = new Point(610, 60), Size = new Size(100, 30), BackColor = Color.LightSkyBlue };
            btnSaveEdit.Click += BtnSaveEdit_Click;
            btnSaveEdit.Visible = false; // Hidden until the user clicks Edit Selected

            // Admin Only Button
            btnManageUsers = new Button { Text = "Manage Users", Location = new Point(730, 20), Size = new Size(120, 70), BackColor = Color.LightBlue };
            btnManageUsers.Click += BtnManageUsers_Click;
            btnManageUsers.Visible = _currentUser.UserRole == "Admin"; // Hide if Business

            // DataGridView for Events
            dgvEvents = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(740, 420),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            // Add controls
            this.Controls.Add(lblName); this.Controls.Add(txtEventName);
            this.Controls.Add(lblDate); this.Controls.Add(dtpEventDate);
            this.Controls.Add(lblType); this.Controls.Add(cmbEventType);
            this.Controls.Add(lblPrice); this.Controls.Add(numPrice);
            this.Controls.Add(btnAddEvent); this.Controls.Add(btnDeleteEvent);
            this.Controls.Add(btnEditEvent); this.Controls.Add(btnSaveEdit);
            this.Controls.Add(btnManageUsers);
            this.Controls.Add(dgvEvents);
        }

        private void LoadEvents()
        {
            var allEvents = _eventSystem.GetAllEvents();
            
            // Filter logic based on role
            if (_currentUser.UserRole == "Verified_Business")
            {
                // NOTE: This requires you to add 'BusinessID' to your WembleyEvent class!
                // dgvEvents.DataSource = allEvents.Where(e => e != null && e.BusinessID == _currentUser.UserID).ToArray();
            }
            else // Admin
            {
                dgvEvents.DataSource = allEvents.Where(e => e != null).ToArray();
            }
        }

        private void BtnAddEvent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            WembleyEvent newEvent = new WembleyEvent(
                0, // ID auto-generates in your system
                txtEventName.Text,
                dtpEventDate.Value,
                cmbEventType.SelectedItem.ToString(),
                0, // Initial attendance
                (int)numPrice.Value
            );

            // newEvent.BusinessID = _currentUser.UserID; // Don't forget to set this once you update the model!

            _eventSystem.AddEvent(newEvent);
            LoadEvents(); // Refresh Grid
            MessageBox.Show("Event Added Successfully!");
        }

        private void BtnDeleteEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;

                // This is used to ask the user for confirmation before deleting an event
                DialogResult confirm = MessageBox.Show(
                    "Are you sure you want to delete this event?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                // This is used to only delete the event if the user clicks Yes
                if (confirm == DialogResult.Yes)
                {
                    _eventSystem.DeleteEvent(selectedEvent.EventID);
                    LoadEvents(); // Refresh Grid
                }
            }
        }

        // This is used to load the selected event's details into the input fields so the user can edit them
        private void BtnEditEvent_Click(object sender, EventArgs e)
        {
            if (dgvEvents.SelectedRows.Count > 0)
            {
                var selectedEvent = (WembleyEvent)dgvEvents.SelectedRows[0].DataBoundItem;

                // This is used to populate the input fields with the current event values
                txtEventName.Text = selectedEvent.EventName;
                dtpEventDate.Value = selectedEvent.EventDate;
                cmbEventType.SelectedItem = selectedEvent.EventType;
                numPrice.Value = selectedEvent.EventPrice;

                // This is used to remember which event is being edited so Save Edit knows what to update
                _editingEventID = selectedEvent.EventID;

                // This is used to show the Save Edit button so the user can confirm their changes
                btnSaveEdit.Visible = true;

                MessageBox.Show("Fields loaded. Edit the values above and click Save Edit to save.", "Editing Event", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select an event to edit.");
            }
        }

        // This is used to save the edited event details back to the system
        private void BtnSaveEdit_Click(object sender, EventArgs e)
        {
            if (_editingEventID == -1)
            {
                MessageBox.Show("No event selected for editing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEventName.Text) || cmbEventType.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields before saving.");
                return;
            }

            // This is used to build the updated event with the new values from the input fields
            WembleyEvent updatedEvent = new WembleyEvent(
                _editingEventID,
                txtEventName.Text,
                dtpEventDate.Value,
                cmbEventType.SelectedItem.ToString(),
                0, // Attendance is preserved by UpdateEvent internally
                (int)numPrice.Value
            );

            _eventSystem.UpdateEvent(_editingEventID, updatedEvent);
            LoadEvents(); // Refresh Grid

            // This is used to reset the editing state after saving
            _editingEventID = -1;
            btnSaveEdit.Visible = false;

            MessageBox.Show("Event updated successfully!");
        }

        private void BtnManageUsers_Click(object sender, EventArgs e)
        {
            ManageUser.ManageUsersForm usersForm = new ManageUser.ManageUsersForm(_userSystem);
            usersForm.ShowDialog();
        }
    }
}