
using System.Collections.Generic;
using System;
using System.Linq;
using static Hosp1.Hospital;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using static Hosp1.Hospital.Surgeon;


namespace Hosp1
{
    // Entry point of the application
    public class Program
    {
        public static void Main(string[] args)
        {
            Hospital hospital = new Hospital();

            hospital.Start();
        }
    }

    public class Hospital
    {
        private List<User> users = new List<User>();//Listing all users of the hospital app(patients, Floor managers and surgeons)
        private List<Room> rooms = new List<Room>();// List of all rooms in the hospital

        // Method to get all patients from the list of users
        private List<Patient> GetPatients()
        {
            List<Patient> patients = new List<Patient>();
            foreach (var user in users)
            {
                if (user is Patient patient)// Check if user is a patient
                {
                    patients.Add(patient);// Adding patient to the patients list
                }
            }
            return patients;
        }

        //Dictionary that tracks room-floor mapping and if they are occuppied 
        private Dictionary<(int room, int floor), bool> roomFloorMapping = new Dictionary<(int, int), bool>();

        //Constructor to initialize the hospital with rooms and setting the room-floor mapping
        public Hospital()
        {
            // Initialize 60 rooms, 10 rooms per floor, across 6 floors
            for (int roomNum = 1; roomNum <= 60; roomNum++)
            {
                // Calculate the floor number based on the room number
                int floorNum = (roomNum - 1) / 10 + 1; // 10 rooms per floor, so floor is calculated this way

                // Initialize rooms
                rooms.Add(new Room(roomNum));

                // Map the room to the corresponding floor and set it as not occupied
                roomFloorMapping[(roomNum, floorNum)] = false; // false means the room is not occupied
            }
        }

        private string GetValidatedInput(string prompt, string errorMessage, string[] validOptions = null)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                if (!string.IsNullOrWhiteSpace(input) && (validOptions == null || validOptions.Contains(input)))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine($"#Error - {errorMessage}");
                    Console.WriteLine("#####\n");

                    // Pause to ensure the user has time to see the error message
                    System.Threading.Thread.Sleep(1000); 
                }
            }
        }

        // Starting the hospital application
        public void Start()
        {
            
            Console.WriteLine("=================================");
            Console.WriteLine("Welcome to Gardens Point Hospital");
            Console.WriteLine("=================================");

            //Infinite loop to keep the application running until the user wants to exit
            while (true)
            {
                ShowMainMenu();// Display the main menu

                string choice = Console.ReadLine().Trim();// Getting the user's choice

                switch (choice)
                {
                    case "1":
                        Console.WriteLine();
                        Login(); // Call the Login method to handle login functionality
                        break;

                    case "2":
                        RegisterUser();// Call Register User to handle user registration
                        break;

                    case "3":
                        Console.WriteLine();
                        Console.WriteLine("Goodbye. Please stay safe.");
                        return;// Exit application

                    default:
                        Console.WriteLine("#####");
                        Console.WriteLine("#Error - Invalid Menu Option, please try again.");
                        Console.WriteLine("#####");
                        break;
                }
            }
        }

        //Method to check if a specific room is available on every floor
        private bool IsRoomAvailable(int roomNumber, int floorNumber)
        {
            (int room, int floor) roomKey = (roomNumber, floorNumber); // Creating a tuple respresenting a room and floor
            return roomFloorMapping.ContainsKey(roomKey) && !roomFloorMapping[roomKey]; // Return true if the room exists and is not occupied
        }

        //Method to get all surgeons from a list of users
        private List<Surgeon> GetSurgeons()
        {
            List<Surgeon> surgeons = new List<Surgeon>(); // Check if user is a Surgeon
            foreach (var user in users)
            {
                if (user is Surgeon surgeon)// Add surgeon to list
                {
                    surgeons.Add(surgeon);
                }
            }
            return surgeons;
        }



        private void ShowMainMenu() //The main menu being call in the Start method
        {
            Console.WriteLine("\nPlease choose from the menu below:");
            Console.WriteLine("1. Login as a registered user");
            Console.WriteLine("2. Register as a new user");
            Console.WriteLine("3. Exit");
            Console.Write("Please enter a choice between 1 and 3: ");
            Console.WriteLine();
        }
        private void RegisterUser()// Method to register a new user
        {
            Console.Clear(); // Clear screen between actions
            Console.WriteLine("\nRegister as which type of user:");
            Console.WriteLine("1. Patient");
            Console.WriteLine("2. Staff");
            Console.WriteLine("3. Return to the first menu");
            Console.Write("Please enter a choice between 1 and 3: ");

            string userType = Console.ReadLine().Trim();

            switch (userType)
            {
                case "1":
                    RegisterPatient();
                    break;
                case "2":
                    RegisterStaff();
                    break;
                case "3":
                    return; // Return to the main menu
                default:
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Invalid Menu Option, please try again.");
                    Console.WriteLine("#####\n");
                    break;
            }
            
        }

        private void FloorManagerMenu(FloorManager floorManager)// Method to register Floormanagers
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nFloor Manager Menu.");
                Console.WriteLine("Please choose from the menu below: ");
                Console.WriteLine("1. Display my details");
                Console.WriteLine("2. Change password");
                Console.WriteLine("3. Assign room to patient");
                Console.WriteLine("4. Assign surgery");
                Console.WriteLine("5. Unassign room");
                Console.WriteLine("6. Log out");
                Console.Write("Please enter a choice between 1 and 6: ");

                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nYour details.");
                        floorManager.DisplayDetails();
                        break;
                    case "2":
                        ChangeFloorManagerPassword(floorManager);
                        break;
                    case "3":
                        AssignRoomToPatient(floorManager);
                        break;
                    case "4":
                        AssignSurgeryToPatient(floorManager);
                        break;
                    case "5":
                        UnassignRoom(floorManager);
                        break;
                    case "6":
                        Console.WriteLine($"\nFloor manager {floorManager.Name} has logged out.");
                        return;  // Exit the menu and return to the main menu
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine();  
            }
            
        }

        private void Login()
        {
            Console.Clear();
            Console.WriteLine("Login Menu.");
            if (users.Count == 0)
            {
                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - There are no people registered.");
                Console.WriteLine("#####\n");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            string email = GetValidatedInput("Please enter in your email: ", "Email cannot be empty.").Trim();
            Console.WriteLine();

            var user = users.SingleOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - Email is not registered.");
                Console.WriteLine("#####\n");
                System.Threading.Thread.Sleep(1000);
                return; // Go back to the main menu automatically
            }

            string password = GetValidatedInput("Please enter in your password: ", "Password cannot be empty.").Trim();
            Console.WriteLine();

            if (user.Password != password)
            {
                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - Wrong Password.");
                Console.WriteLine("#####\n");
                System.Threading.Thread.Sleep(1000);
                return; // Go back to the main menu automatically
            }



            // Successful login
            Console.WriteLine($"\nHello {user.Name} welcome back.");


            // Direct user to the appropriate menu based on their role
            switch (user)
            {
                case FloorManager floorManager:
                    FloorManagerMenu(floorManager);
                    break;
                case Surgeon surgeon:
                    SurgeonMenu(surgeon);
                    break;
                case Patient patient:
                    PatientMenu(patient);
                    break;
                default:
                    break;
            }
        }

        private void AssignRoomToPatient(FloorManager floorManager)
        {
            // Patients who have undergone surgery cannot check in again
            List<Patient> allPatients = GetPatients().Where(p => !p.HasSurgery).ToList();

            // Check if there are any registered patients at all
            if (allPatients.Count == 0)
            {
                Console.WriteLine("\nThere are no registered patients.");
                return;
            }

            // Get the list of patients that are checked in but have not yet been assigned a room, and have not undergone surgery
            List<Patient> checkedInPatients = allPatients.Where(p => p.IsCheckedIn && p.RoomNumber == -1).ToList();

            // Check if there are any checked-in patients who haven't been assigned a room
            if (checkedInPatients.Count == 0)
            {
                Console.WriteLine("\nThere are no checked in patients.");
                return;
            }

            // Check if there are any available rooms on the floor
            int floorNumber = floorManager.FloorNumber;
            bool hasAvailableRooms = roomFloorMapping.Keys.Any(k => k.floor == floorNumber && !roomFloorMapping[k]);

            if (!hasAvailableRooms)
            {
                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - All rooms on this floor are assigned. ");
                Console.WriteLine("#####\n");
                return;
            }

            // Display the list of checked-in patients
            Console.WriteLine("\nPlease select your patient: ");
            for (int i = 0; i < checkedInPatients.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {checkedInPatients[i].Name}");
            }

            // Select a patient
            int patientIndex = -1;
            while (true)
            {
                Console.Write($"\nPlease enter a choice between 1 and {checkedInPatients.Count}: ");
                bool validInput = int.TryParse(Console.ReadLine().Trim(), out patientIndex);

                if (validInput && patientIndex > 0 && patientIndex <= checkedInPatients.Count)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied value is out of range, please try again.");
                    Console.WriteLine("#####");
                }
            }

            Patient selectedPatient = checkedInPatients[patientIndex - 1];

            // Prompt for room number limited to the floor manager's floor, until a valid room is entered
            int roomNumber = -1;
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter your room (1-10): ");
                string roomInput = Console.ReadLine().Trim();  // Get room input as a string

                if (int.TryParse(roomInput, out roomNumber) && roomNumber >= 1 && roomNumber <= 10)
                {
                    // Calculate the actual room number for the floor
                    int actualRoomNumber = (floorManager.FloorNumber - 1) * 10 + roomNumber;

                    // Use a tuple for room and floor
                    (int room, int floor) roomKey = (actualRoomNumber, floorManager.FloorNumber);

                    if (roomFloorMapping.ContainsKey(roomKey))
                    {
                        if (!roomFloorMapping[roomKey]) // Check if the room is available
                        {
                            // Room is available, mark it as occupied and assign to patient
                            roomFloorMapping[roomKey] = true;

                            // Assign room to patient
                            selectedPatient.AssignRoom(actualRoomNumber);

                            // Update patient in the list (important to persist changes)
                            UpdatePatientInUsersList(selectedPatient);

                            // Output the correct room number assignment
                            int displayRoomNum = (actualRoomNumber - 1) % 10 + 1; // Calculate display room number for output
                            Console.WriteLine($"Patient {selectedPatient.Name} has been assigned to room number {displayRoomNum} on floor {floorManager.FloorNumber}.");

                            // Immediately return to prevent any further execution
                            return;
                        }
                        else
                        {
                            Console.WriteLine("\n#####");
                            Console.WriteLine("#Error - Room has been assigned to another patient, please try again.");
                            Console.WriteLine("#####");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n#####");
                        Console.WriteLine("#Error - Room is not valid, please try again.");
                        Console.WriteLine("#####");
                    }
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied value is out of range, please try again.");
                    Console.WriteLine("#####");
                }
            }
        }


        private void DisplayRoom(Patient patient)
        {
            if (patient.RoomNumber > 0)
            {
                // Calculate the correct floor and display room number
                int floorNumber = CalculateFloorFromRoom(patient.RoomNumber);
                int displayRoomNum = (patient.RoomNumber - 1) % 10 + 1; // Get room number within the floor (1-10)

                Console.WriteLine($"\nYour room is number {displayRoomNum} on floor {floorNumber}.");
            }
            else
            {
                Console.WriteLine($"You do not have an assigned room.");
            }
        }


        private void CheckOutPatient(FloorManager floorManager)
        {
            // Get the list of patients who are checked in and currently occupying rooms
            List<Patient> patients = GetPatients().Where(p => p.IsCheckedIn && p.RoomNumber != -1).ToList();

            if (patients.Count == 0)
            {
                Console.WriteLine("There are no patients currently checked in.");
                return;
            }

            Console.WriteLine("\nPlease select the patient to check out: ");
            for (int i = 0; i < patients.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {patients[i].Name}");
            }

            int patientIndex = -1;
            while (true)
            {
                Console.Write($"\nPlease enter a choice between 1 and {patients.Count}: ");
                bool validInput = int.TryParse(Console.ReadLine().Trim(), out patientIndex);

                if (validInput && patientIndex > 0 && patientIndex <= patients.Count)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine($"# Error: Invalid input. Please enter a number between 1 and {patients.Count}.");
                    Console.WriteLine("#####\n");

                    // Pause to allow the user to see the error message before retrying
                    System.Threading.Thread.Sleep(1000); // Optional: Pause for 1 second
                }
            }

            // Get the selected patient
            Patient selectedPatient = patients[patientIndex - 1];

            if (!selectedPatient.HasSurgery)
            {
                Console.WriteLine("\nYou are unable to check out at this time.");
                return; // Exit the method if the surgery has not been completed
            }

            // Calculate the floor from the current room before unassigning it
            int floorNumber = CalculateFloorFromRoom(selectedPatient.RoomNumber);

            // Update room availability in the roomFloorMapping
            (int room, int floor) roomKey = (selectedPatient.RoomNumber, floorNumber);
            if (roomFloorMapping.ContainsKey(roomKey))
            {
                roomFloorMapping[roomKey] = false; // Mark the room as available again
            }

            // Update patient status after checkout
            selectedPatient.IsCheckedIn = false;  // Reset the check-in status
            selectedPatient.RoomNumber = -1;  // Remove room assignment
            selectedPatient.SurgeonName = "Not assigned";  // Clear surgeon assignment
            selectedPatient.SurgeryDateTime = "Not scheduled";  // Clear surgery date
            selectedPatient.HasSurgery = false;  // Reset surgery status
            selectedPatient.IsEligibleForCheckIn = false;  // Patient is not eligible to check in again immediately after surgery

            // Update patient in the users list
            UpdatePatientInUsersList(selectedPatient);

            Console.WriteLine($"\nPatient {selectedPatient.Name} has successfully checked out and their room is now available.");
        }


        private void UpdatePatientInUsersList(Patient updatedPatient)
        {
            int index = users.FindIndex(u => u is Patient p && p.Email == updatedPatient.Email);
            if (index != -1)
            {
                users[index] = updatedPatient;
            }
            else
            {
                // Console.WriteLine("DEBUG: Error updating patient - patient not found in user list.");
            }
        }
        // Function to calculate the floor based on room number
        private int CalculateFloorFromRoom(int roomNumber)
        {
            return ((roomNumber - 1) / 10) + 1;  // Adjust the logic as per your floor-room mapping
        }

        private void AssignSurgeryToPatient(FloorManager floorManager)
        {
            List<Patient> allPatients = GetPatients().OfType<Patient>().ToList();

            if (allPatients.Count == 0)
            {
                DisplayError("There are no registered patients.");
                return;
            }

            // Get the list of patients that are checked in, have assigned rooms, and have not yet been scheduled for surgery
            List<Patient> availablePatients = allPatients
                .Where(p => p.IsCheckedIn && p.RoomNumber != -1 && !p.HasSurgery)
                .ToList();

            if (availablePatients.Count == 0)
            {
                DisplayError("There are no patients ready for surgery.");
                return;
            }

            Console.WriteLine("\nPlease select your patient:");
            for (int i = 0; i < availablePatients.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {availablePatients[i].Name}");
            }

            int patientIndex = GetValidatedIntInput($"\nPlease enter a choice between 1 and {availablePatients.Count}: ", "Supplied value is out of range, please try again.", 1, availablePatients.Count) - 1;
            Patient selectedPatient = availablePatients[patientIndex];

            List<Surgeon> surgeons = GetSurgeons();
            if (surgeons.Count == 0)
            {
                DisplayError("No surgeons are registered");
                return;
            }

            Console.WriteLine("\nPlease select your surgeon:");
            for (int i = 0; i < surgeons.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {surgeons[i].Name}");
            }

            int surgeonIndex = GetValidatedIntInput($"\nPlease enter a choice between 1 and {surgeons.Count}: ", "Supplied value is out of range, please try again.", 1, surgeons.Count) - 1;
            Surgeon selectedSurgeon = surgeons[surgeonIndex];

            DateTime surgeryDateTime;
            while (true)
            {
                Console.Write("\nPlease enter a date and time (e.g. 14:30 31/01/2024).");
                string surgeryDateTimeInput = Console.ReadLine().Trim();

                if (DateTime.TryParseExact(surgeryDateTimeInput, "HH:mm dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out surgeryDateTime))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied value is not a valid DateTime.");
                    Console.WriteLine("#####\n");
                    Thread.Sleep(1000);
                }
            }

            // Assign surgery to the patient
            selectedPatient.AssignSurgeon(selectedSurgeon.Name, surgeryDateTime.ToString("HH:mm dd/MM/yyyy"));
            selectedSurgeon.AddSurgery(selectedPatient);
            selectedPatient.HasSurgery = true;  // Mark the patient as having a scheduled surgery
            UpdatePatientInUsersList(selectedPatient);

            Console.WriteLine($"\nSurgeon {selectedSurgeon.Name} has been assigned to patient {selectedPatient.Name}.");
            Console.WriteLine($"Surgery will take place on {surgeryDateTime:HH:mm dd/MM/yyyy}.");
        }



        // Method to get a list of all patients
        private List<Patient> GetAllPatients()
        {
            // Filters and returns all users who are of type Patient
            return users.OfType<Patient>().ToList();
        }

        // Method to save all user data to a CSV file for data persistence
        private void SaveUsersToCSV()
        {
            // Using StreamWriter to write user data to a CSV file
            using (StreamWriter writer = new StreamWriter("users.csv"))
            {
                // Iterate over each user in the users list
                foreach (var user in users)
                {
                    if (user is Patient patient)
                    {
                        writer.WriteLine($"Patient,{patient.Name},{patient.Age},{patient.Mobile},{patient.Email},{patient.Password},{patient.RoomNumber},{patient.SurgeonName},{patient.SurgeryDateTime}");
                    }
                    else if (user is FloorManager floorManager)
                    {
                        writer.WriteLine($"FloorManager,{floorManager.Name},{floorManager.Age},{floorManager.Mobile},{floorManager.Email},{floorManager.Password},{floorManager.StaffId},{floorManager.FloorNumber}");
                    }
                    else if (user is Surgeon surgeon)
                    {
                        writer.WriteLine($"Surgeon,{surgeon.Name},{surgeon.Age},{surgeon.Mobile},{surgeon.Email},{surgeon.Password},{surgeon.StaffId},{surgeon.Specialty}");
                    }
                }
            }

        }

        // Method to load user data from a CSV file
        private void LoadUsersFromCSV()
        {
            // Check if the CSV file exists before attempting to load data
            if (File.Exists("users.csv"))
            {
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines("users.csv");
                foreach (var line in lines)
                {
                    // Split each line by comma to extract individual properties
                    string[] parts = line.Split(',');
                    string userType = parts[0]; // Identify the type of user (Patient, FloorManager, Surgeon)

                    // Use a switch statement to determine the user type and create the corresponding object
                    switch (userType)
                    {
                        // Ensure the correct number of fields is present for a Patient
                        case "Patient":
                            if (parts.Length == 9)
                            {
                                // Extract Patient data from parts array
                                string name = parts[1];
                                int age = int.Parse(parts[2]);
                                string mobile = parts[3];
                                string email = parts[4];
                                string password = parts[5];
                                int roomNumber = int.Parse(parts[6]);
                                string surgeonName = parts[7];
                                string surgeryDateTime = parts[8];

                                // Create a new Patient object and add it to the users list
                                Patient patient = new Patient(name, age, mobile, email, password)
                                {
                                    RoomNumber = roomNumber,
                                    SurgeonName = surgeonName,
                                    SurgeryDateTime = surgeryDateTime
                                };
                                users.Add(patient);
                            }
                            break;

                        case "FloorManager":
                            // ensure the correct number of fields is present for a FloorManager
                            if (parts.Length == 8)
                            {
                                string name = parts[1];
                                int age = int.Parse(parts[2]);
                                string mobile = parts[3];
                                string email = parts[4];
                                string password = parts[5];
                                string staffId = parts[6];
                                int floorNumber = int.Parse(parts[7]);

                                // Create a new FloorManager object and add it to the users list
                                FloorManager floorManager = new FloorManager(name, age, mobile, email, password, staffId, floorNumber);
                                users.Add(floorManager);
                            }
                            break;

                        case "Surgeon":
                            //Ensure the correct number of fields is present for a Surgeon
                            if (parts.Length == 8)
                            {
                                // Extract Surgeon data from parts array
                                string name = parts[1];
                                int age = int.Parse(parts[2]);
                                string mobile = parts[3];
                                string email = parts[4];
                                string password = parts[5];
                                string staffId = parts[6];
                                string specialty = parts[7];

                                // Create a new Surgeon object and add it to the users list
                                Surgeon surgeon = new Surgeon(name, age, mobile, email, password, staffId, specialty);
                                users.Add(surgeon);
                            }
                            break;

                        default:
                            Console.WriteLine($"Unknown user type: {userType}");
                            break;
                    }
                }
            }
        }

        private void UnassignRoom(FloorManager floorManager)
        {
            // Get patients who are not checked in but still have assigned rooms.
            List<Patient> patients = GetPatients().Where(p => !p.IsCheckedIn && p.RoomNumber != -1).ToList();

            if (patients.Count == 0)
            {
                Console.WriteLine("There are no patients ready to have their rooms unassigned.");
                return;
            }

            Console.WriteLine("\nPlease select your patient:");
            for (int i = 0; i < patients.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {patients[i].Name}");
            }

            int patientIndex;
            while (true)
            {
                Console.Write($"\nPlease enter a choice  between 1 and {patients.Count}.");
                bool validInput = int.TryParse(Console.ReadLine().Trim(), out patientIndex);

                if (validInput && patientIndex >= 1 && patientIndex <= patients.Count)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine($"# Error: Invalid input. Please enter a number between 1 and {patients.Count}.");
                    Console.WriteLine("#####\n");

                    // Optional pause to ensure the user can read the error message
                    System.Threading.Thread.Sleep(1000);
                }
            }

            Patient selectedPatient = patients[patientIndex - 1];

            // Proceed with unassigning the room
            int roomNumber = selectedPatient.RoomNumber;
            int floorNumber = CalculateFloorFromRoom(roomNumber);
            int displayRoomNumber = (roomNumber - 1) % 10 + 1;

            // Unassign the room
            selectedPatient.UnassignRoom();
            roomFloorMapping[(roomNumber, floorNumber)] = false; // Mark the room as available again
            UpdatePatientInUsersList(selectedPatient);

            Console.WriteLine($"\nRoom number {displayRoomNumber} on floor {floorNumber} has been unassigned.");
        }


        // This method removes a patient from all surgeons' upcoming surgery lists
        private void RemovePatientFromSurgeons(Patient patient)
        {
            foreach (var user in users)
            {
                if (user is Surgeon surgeon && surgeon.UpcomingSurgeries != null)
                {
                    if (surgeon.UpcomingSurgeries.Contains(patient))
                    {
                        surgeon.UpcomingSurgeries.Remove(patient);
                        Console.WriteLine($"Patient {patient.Name} has been removed from surgeon {surgeon.Name}'s upcoming surgeries.");
                    }
                }
            }

            // Update patient state after removal to ensure consistency
            patient.HasSurgery = false;
            patient.SurgeonName = "Not assigned";
            patient.SurgeryDateTime = "Not scheduled";
        }


        private Patient FindPatientByEmail(string email)
        {
            foreach (var user in users)
            {
                if (user is Patient patient && patient.Email == email)
                {
                    return patient;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns an available surgeon from the list of users.
        /// </summary>
        /// <returns>An instance of <see cref="Surgeon"/> if one is available, otherwise <c>null</c>.</returns>
        private Surgeon FindAvailableSurgeon()
        {
            foreach (var user in users)
            {
                if (user is Surgeon surgeon)
                {
                    return surgeon;
                }
            }
            return null;
        }

        /// <summary>
        /// Changes the password for a specified patient.
        /// </summary>
        /// <param name="patient">The patient whose password is to be changed.</param>
        private void ChangePatientPassword(Patient patient)
        {
            Console.Write("Enter new password: ");
            string newPassword = Console.ReadLine().Trim();  // Input new password
            Console.WriteLine();

            // Update password without checking the current password in this example
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                patient.ChangePassword(patient.Password, newPassword);  // Change password directly
                Console.WriteLine("Password has been changed.");  // Confirmation message
            }
            else
            {
                Console.WriteLine("Password change failed. New password cannot be empty.");
            }
            
        }

        /// <summary>
        /// Allows a floor manager to change their password by validating the current password and setting a new one.
        /// </summary>
        /// <param name="floorManager">The floor manager whose password is to be changed.</param>
        private void ChangeFloorManagerPassword(FloorManager floorManager)
        {
            Console.WriteLine(); // Add a line break before starting the password prompt
            Console.Write("Enter new password: ");
            string currentPassword = Console.ReadLine().Trim();

            if (floorManager.Password == currentPassword)
            {
                Console.WriteLine(); // Add a line break after successful password validation
                Console.Write("Please enter your new password: ");
                string newPassword = Console.ReadLine().Trim();

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    floorManager.ChangePassword(currentPassword, newPassword);
                    Console.WriteLine("\nPassword has been changed successfully.\n"); // Line breaks before and after the success message
                }
                else
                {
                    Console.WriteLine("\nNew password cannot be empty.\n"); // Add newline for better separation
                }
            }
            else
            {
                Console.WriteLine("\nPassword has been changed. \n");
            }
        }

        /// <summary>
        /// Displays the patient menu and provides options for interacting with patient details.
        /// </summary>
        /// <param name="patient">The patient for whom the menu is being displayed.</param>
        private void PatientMenu(Patient patient)
        {
            while (true)
            {
                Console.WriteLine("\nPatient Menu.");
                Console.WriteLine("Please choose from the menu below:");
                Console.WriteLine("1. Display my details");
                Console.WriteLine("2. Change password");
                Console.WriteLine(patient.IsCheckedIn ? "3. Check out" : "3. Check in");
                Console.WriteLine("4. See room");
                Console.WriteLine("5. See surgeon");
                Console.WriteLine("6. See surgery date and time");
                Console.WriteLine("7. Log out");
                Console.Write("Please enter a choice between 1 and 7: \n");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayPatientDetails(patient);
                        break;
                    case "2":
                        ChangePatientPassword(patient);
                        break;
                    case "3":
                        if (patient.IsCheckedIn)
                        {
                            if (patient.HasSurgery)
                            {
                                Console.WriteLine($"\nPatient {patient.Name} has been checked out.");
                                patient.CheckOut(roomFloorMapping);
                            }
                            else
                            {
                                Console.WriteLine("\nYou are unable to check out at this time.");
                            }
                        }
                        else
                        {
                            if (!patient.IsEligibleForCheckIn)
                            {
                                Console.WriteLine("\nYou are unable to check in at this time.");
                            }
                            else
                            {
                                Console.WriteLine($"\nPatient {patient.Name} has been checked in.");
                                patient.IsCheckedIn = true;  // Set the patient as checked in
                                patient.IsEligibleForCheckIn = false;  // Patient is now checked in, and needs surgery to be eligible to check out
                            }
                        }
                        UpdatePatientInUsersList(patient);
                        break;
                    case "4":
                        DisplayRoom(patient);  // Use the DisplayRoom method here
                        break;
                    case "5":  // New case for showing the assigned surgeon
                        if (!string.IsNullOrEmpty(patient.SurgeonName) && patient.SurgeonName != "Not assigned")
                        {
                            Console.WriteLine($"Your surgeon is {patient.SurgeonName}.");
                        }
                        else
                        {
                            Console.WriteLine("You do not have an assigned surgeon.");
                        }
                        break;
                    case "6":
                        if (patient.SurgeryDateTime == "Not scheduled")
                        {
                            Console.WriteLine("You do not have assigned surgery.");
                        }
                        else
                        {
                            Console.WriteLine($"Your surgery time is {patient.SurgeryDateTime}.");
                        }
                        break;
                    case "7":
                        Console.WriteLine($"\nPatient {patient.Name} has logged out.");
                        return;  // Go back to the main menu after logout
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the room assignment details for a specific patient, including the room number and floor.
        /// </summary>
        /// <param name="patient">The patient whose room assignment details are to be displayed.</param>
        private void DisplayRoomDetails(Patient patient)
        {
            if (patient.RoomNumber > 0)
            {
                // Calculate the correct floor and display room number
                int floorNumber = CalculateFloorFromRoom(patient.RoomNumber);
                int displayRoomNum = (patient.RoomNumber - 1) % 10 + 1; // Get room number within the floor (1-10)

                Console.WriteLine($"\nPatient {patient.Name} is assigned to room number {displayRoomNum} on floor {floorNumber}.");
            }
            else
            {
                Console.WriteLine($"Patient {patient.Name} does not have an assigned room.");
            }
        }

        /// <summary>
        /// Retrieves a list of patients who are currently checked in.
        /// </summary>
        /// <returns>A list of patients who are checked in.</returns>
        private List<Patient> GetCheckedInPatients()
        {
            return GetPatients().Where(p => p.IsCheckedIn).ToList();
        }

        /// <summary>
        /// Displays the surgeon's menu and processes user input to navigate through different actions, such as displaying details, managing patients, and performing surgeries.
        /// </summary>
        /// <param name="surgeon">The surgeon whose menu is being displayed.</param>
        private void SurgeonMenu(Surgeon surgeon)
        {
            while (true)
            {
                Console.WriteLine("\nSurgeon Menu.");
                Console.WriteLine("Please choose from the menu below:");
                Console.WriteLine("1. Display my details");
                Console.WriteLine("2. Change password");
                Console.WriteLine("3. See your list of patients");
                Console.WriteLine("4. See your schedule");
                Console.WriteLine("5. Perform surgery");
                Console.WriteLine("6. Log out");
                Console.Write("Please enter a choice between 1 and 6: ");

                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nYour details.");
                        surgeon.DisplayDetails();
                        break;
                    case "2":
                        ChangeSurgeonPassword(surgeon);
                        break;
                    case "3":
                        ShowSurgeonPatients(surgeon);
                        break;
                    case "4":
                        surgeon.SeeYourSchedule();
                        break;
                    case "5":
                        PerformSurgery(surgeon);
                        break;
                    case "6":
                        Console.WriteLine($"\nSurgeon {surgeon.Name} has logged out.");
                        return;  // Exit the menu and return to the main menu
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine();  // Add space between interactions
            }
        }

        /// <summary>
        /// Displays the surgeon's upcoming surgery schedule, including patient names and scheduled surgery times.
        /// </summary>
        /// <param name="surgeon">The surgeon whose schedule needs to be displayed.</param>
        private void ShowSurgeonSchedule(Surgeon surgeon)
        {
            Console.WriteLine("\nYour schedule.");

            if (surgeon.UpcomingSurgeries.Count > 0)
            {
                int surgeryCount = 1;
                foreach (var patient in surgeon.UpcomingSurgeries)
                {
                    Console.WriteLine($"Performing surgery on patient {patient.Name} on {patient.SurgeryDateTime}");
                    surgeryCount++;
                }
            }
            else
            {
                Console.WriteLine("You do not have any patients assigned.");
            }
        }

        /// <summary>
        /// Displays the list of patients assigned to the surgeon.
        /// </summary>
        /// <param name="surgeon">The surgeon whose assigned patients need to be displayed.</param>
        private void ShowSurgeonPatients(Surgeon surgeon)
        {
            Console.WriteLine("\nYour Patients.");

            bool hasPatients = false;
            int patientCount = 1;

            foreach (var user in users)
            {
                if (user is Patient patient && patient.SurgeonName == surgeon.Name)
                {
                    hasPatients = true;
                    Console.WriteLine($"{patientCount}. {patient.Name}");
                    patientCount++;
                }
            }

            if (!hasPatients)
            {
                Console.WriteLine("You do not have any patients assigned. ");
            }
        }


        /// <summary>
        /// Allows the surgeon to change their password by entering a new valid password.
        /// </summary>
        /// <param name="surgeon">surgeon whose password needs to be changed.</param>
        private void ChangeSurgeonPassword(Surgeon surgeon)
        {
            Console.Write("Enter new password: ");
            string newPassword = Console.ReadLine().Trim();

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                surgeon.ChangePassword(surgeon.Password, newPassword);  // Update the password
                Console.WriteLine("Password has been changed.");
            }
            else
            {
                Console.WriteLine("Password change failed. New password cannot be empty.");
            }
        }

        /// <summary>
        /// Allows the surgeon to perform a surgery by removing the completed surgery from the upcoming surgeries list.
        /// </summary>
        /// <param name="surgeon">The surgeon who is performing the surgery.</param>
        private void PerformSurgery(Surgeon surgeon)
        {
            if (surgeon.UpcomingSurgeries.Count == 0)
            {
                Console.WriteLine("\nNo surgeries are scheduled for this surgeon.");
                return;
            }

            Console.WriteLine("\nPlease select your patient:");
            for (int i = 0; i < surgeon.UpcomingSurgeries.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {surgeon.UpcomingSurgeries[i].Name}");
            }

            int patientChoice;
            while (true)
            {
                Console.Write($"\nPlease enter a choice between 1 and {surgeon.UpcomingSurgeries.Count}: ");
                if (int.TryParse(Console.ReadLine().Trim(), out patientChoice) && patientChoice >= 1 && patientChoice <= surgeon.UpcomingSurgeries.Count)
                {
                    break; // Valid choice, exit loop
                }

                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - Supplied value is out of range, please try again.");
                Console.WriteLine("#####\n");
            }

            Patient selectedPatient = surgeon.UpcomingSurgeries[patientChoice - 1];
            selectedPatient.MarkSurgeryComplete();
            surgeon.UpcomingSurgeries.Remove(selectedPatient);
            UpdatePatientInUsersList(selectedPatient);

            Console.WriteLine($"\nSurgery performed on {selectedPatient.Name} by {surgeon.Name}.\n");
        }

        /// <summary>
        /// Displays the details of a specified patient, including personal information and any assigned room, surgeon, or surgery details.
        /// </summary>
        /// <param name="patient">The patient whose details need to be displayed.</param>
        private void DisplayPatientDetails(Patient patient)
        {
            Console.WriteLine("\nYour details.");
            Console.WriteLine($"Name: {patient.Name}");
            Console.WriteLine($"Age: {patient.Age}");
            Console.WriteLine($"Mobile phone: {patient.Mobile}");
            Console.WriteLine($"Email: {patient.Email}");

            // Only display Room Number if it is assigned
            if (patient.RoomNumber != 0)
            {


            }

            // Only display Surgeon if it is assigned
            if (patient.SurgeonName != "Not assigned")
            {
                Console.WriteLine($"Surgeon: {patient.SurgeonName}");
            }

            // Only display Surgery Date and Time if it is scheduled
            if (patient.SurgeryDateTime != "Not scheduled")
            {
                Console.WriteLine($"Surgery Date and Time: {patient.SurgeryDateTime}");
            }
        }

        /// <summary>
        /// Registers a new patient by collecting required information such as name, age, mobile number, email, and password.
        /// </summary>
        private void RegisterPatient()
        {
            Console.WriteLine("\nRegistering as a patient.");
            string name = GetValidatedName("Please enter in your name: ", "Supplied name is invalid, please try again.");
            Console.WriteLine();
            int age = GetValidatedIntInput("Please enter in your age: ", "Supplied age is invalid, please try again. ", 0, 100);
            Console.WriteLine();
            string mobile = GetValidatedMobileNumber("Please enter in your mobile number: ", "Supplied mobile number is invalid, please try again.");
            Console.WriteLine();
            string email = GetValidatedEmail("Please enter in your email: ", "Supplied email is invalid, please try again.");
            Console.WriteLine();
            string password = GetValidatedPassword("Please enter in your password: ", "Supplied password is invalid, please try again. ");
            Console.WriteLine();

            if (EmailExists(email))
            {
                Console.WriteLine("\n#####");
                Console.WriteLine("#Error - Email is already registered, please try again.");
                Console.WriteLine("#####\n");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            Patient newPatient = new Patient(name, age, mobile, email, password);
            users.Add(newPatient);
            Console.WriteLine($"\n{name} is registered as a patient.");
        }

        /// <summary>
        /// Prompts user for an email address, validates its format, and ensures it is unique.
        /// </summary>
        /// <param name="prompt"> message displayed to the user to request email input.</param>
        /// <param name="errorMessage">error message displayed if the email does not meet the required format or is already registered.</param>
        /// <returns>validated email address entered by the user.</returns>
        private string GetValidatedEmail(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                // Check if the email contains exactly one '@' character and at least one character on either side
                if (input.Contains('@') && input.IndexOf('@') > 0 && input.IndexOf('@') < input.Length - 1)
                {
                    if (!EmailExists(input))
                    {
                        return input;
                    }
                    else
                    {
                        Console.WriteLine("\n#####");
                        Console.WriteLine("#Error - Email is already registered, please try again.");
                        Console.WriteLine("#####\n");
                    }
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine($"#Error - {errorMessage}");
                    Console.WriteLine("#####\n");
                }

                // Pause to ensure the user has time to see the error message
                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Prompts user for a password, validates it based on certain criteria, and ensures it meets the security requirements.
        /// </summary>
        /// <param name="prompt">message displayed to the user to request password input.</param>
        /// <param name="errorMessage">error message displayed if the password does not meet the required criteria.</param>
        /// <returns>validated password entered by the user.</returns>
        private string GetValidatedPassword(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                // Check if the password is at least 8 characters long, contains both numbers and letters, and uses mixed case
                if (input.Length >= 8 && input.Any(char.IsDigit) && input.Any(char.IsLetter) && input.Any(char.IsUpper) && input.Any(char.IsLower))
                {
                    return input;
                }

                // Display specific error message for invalid password
                if (input.Length < 8)
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied password is invalid, please try again.");
                    Console.WriteLine("#####\n");
                }
                else if (!input.Any(char.IsDigit))
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied password is invalid, please try again.");
                    Console.WriteLine("#####\n");
                }
                else if (!input.Any(char.IsLetter))
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied password is invalid, please try again.");
                    Console.WriteLine("#####\n");
                }
                else if (!input.Any(char.IsUpper) || !input.Any(char.IsLower))
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied password is invalid, please try again.");
                    Console.WriteLine("#####\n");
                }

                System.Threading.Thread.Sleep(1000); // Pause for 1 second (optional)
            }
        }

        /// <summary>
        /// Prompts user for an integer input, validates it, and ensures it falls within the specified range.
        /// </summary>
        /// <param name="prompt">The message displayed to the user to request input.</param>
        /// <param name="errorMessage">The error message displayed if the input is not valid or out of range.</param>
        /// <param name="minValue">The minimum allowed value for the input (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value for the input (inclusive).</param>
        /// <returns>The validated integer input from user.</returns>
        private int GetValidatedIntInput(string prompt, string errorMessage, int minValue = 0, int maxValue = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                // Check if the input can be parsed as an integer
                if (int.TryParse(input, out int result))
                {
                    // Check if the integer falls within the specified range
                    if (result >= minValue && result <= maxValue)
                    {
                        return result;
                    }
                    else
                    {
                        Console.WriteLine("\n#####");
                        Console.WriteLine($"#Error - {errorMessage}");
                        Console.WriteLine("#####\n");
                    }
                }
                else
                {
                    // Display error message if input is not a valid integer
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied value is not an integer, please try again.");
                    Console.WriteLine("#####\n");
                }

                // Pause to allow the user to read the error before repeating the prompt
                System.Threading.Thread.Sleep(1000); // Pause for 1 second (optional)
            }
        }



        private HashSet<int> assignedFloors = new HashSet<int>();

        /// <summary>
        /// Registers a new staff member (either a floor manager or surgeon) by collecting the required details and validating the input.
        /// </summary>
        private void RegisterStaff()
        {
            Console.Clear();

            // Prompt for staff type before asking for personal details
            Console.WriteLine("\nRegister as which type of staff:");
            Console.WriteLine("1. Floor manager");
            Console.WriteLine("2. Surgeon");
            Console.WriteLine("3. Return to the first menu");
            Console.Write("Please enter a choice between 1 and 3: ");  // Ensure input on the same line

            string staffType = Console.ReadLine().Trim();

            if (staffType == "3")
            {
                return; // Return to the main menu
            }

            if (staffType != "1" && staffType != "2")
            {
                Console.WriteLine("Invalid choice. Returning to the main menu.");
                return;
            }

            if (staffType == "1")
            {
                // Check if all floors are assigned before proceeding
                if (assignedFloors.Count >= 6)
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - All floors are assigned.");
                    Console.WriteLine("#####\n");
                    return;
                }

                Console.WriteLine("\nRegistering as a floor manager.");
            }
            else if (staffType == "2")
            {
                Console.WriteLine("\nRegistering as a surgeon.");
            }

            // Collect the required information
            string name = GetValidatedInput("Please enter in your name: ", "Supplied name is invalid, please try again.");
            Console.WriteLine();
            int age = 0;
            if (staffType == "1")
            {
                age = GetValidatedIntInput("Please enter in your age: ", "Supplied age is invalid, please try again. ", 21, 70);
            }
            else if (staffType == "2")
            {
                age = GetValidatedIntInput("Please enter in your age: ", "Supplied age is invalid, please try again. ", 30, 75);
            }
            Console.WriteLine();
            string mobile = GetValidatedMobileNumber("Please enter in your mobile number: ", "Supplied mobile number is invalid, please try again.");
            Console.WriteLine();
            string email = GetValidatedEmail("Please enter in your email: ", "Supplied email is invalid, please try again.");
            Console.WriteLine();
            string password = GetValidatedPassword("Please enter in your password: ", "Supplied password is invalid, please try again.");
            Console.WriteLine();

            // Prompt for staff ID and validate its uniqueness and range
            int staffId = 0;
            while (true)
            {
                staffId = GetValidatedIntInput("Please enter in your staff ID: ", "Supplied staff identification number is invalid, please try again.", 100, 999);
                Console.WriteLine();
                if (StaffIdExists(staffId.ToString()))
                {
                    Console.WriteLine("#####");
                    Console.WriteLine("#Error - Staff ID is already registered, please try again.");
                    Console.WriteLine("#####\n");
                    System.Threading.Thread.Sleep(1000);
                }
                else
                {
                    break; // Exit the loop if the staff ID is valid and unique
                }
            }

            if (staffType == "1")
            {
                // Floor manager registration
                while (true)
                {
                    int floorNumber = GetValidatedIntInput("Please enter in your floor number: ", "Supplied floor is invalid, please try again.", 1, 6);
                    Console.WriteLine();
                    if (!assignedFloors.Contains(floorNumber))
                    {
                        assignedFloors.Add(floorNumber); // Mark this floor as assigned
                        FloorManager newFloorManager = new FloorManager(name, age, mobile, email, password, staffId.ToString(), floorNumber);
                        users.Add(newFloorManager);

                        Console.WriteLine($"\n{name} is registered as a floor manager.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("\n#####");
                        Console.WriteLine("#Error - Floor has been assigned to another floor manager, please try again.");
                        Console.WriteLine("#####\n");
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            else if (staffType == "2")
            {
                // Surgeon registration
                string specialty = ChooseSurgeonSpecialty();
                Console.WriteLine();

                Surgeon newSurgeon = new Surgeon(name, age, mobile, email, password, staffId.ToString(), specialty);
                users.Add(newSurgeon);

                Console.WriteLine($"\n{name} is registered as a surgeon.");
            }
        }

        /// <summary>
        /// Prompts the user for a name and validates the input to ensure it contains only letters and spaces.
        /// </summary>
        /// <param name="prompt">The prompt message to display to the user.</param>
        /// <param name="errorMessage">The error message to display when the input is invalid.</param>
        /// <returns>A valid name as a string.</returns>
        private string GetValidatedName(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                // Ensure name contains only letters and spaces and is not empty
                if (!string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, "^[a-zA-Z ]+$"))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine($"#Error - {errorMessage}");
                    Console.WriteLine("#####\n");

                    // Pause to ensure the user has time to see the error message
                    System.Threading.Thread.Sleep(1000); // Pause for 1 second (optional)
                }
            }
        }


        /// <summary>
        /// Prompts the user for a mobile number and validates the input.
        /// </summary>
        /// <param name="prompt">The prompt message to display to the user.</param>
        /// <param name="errorMessage">The error message to display when the input is invalid.</param>
        /// <returns>A valid mobile number as a string.</returns>
        private string GetValidatedMobileNumber(string prompt, string errorMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine().Trim();

                // Check if the mobile number is exactly 10 digits, starts with '0', and contains only numbers
                if (input.Length == 10 && input.StartsWith("0") && input.All(char.IsDigit))
                {
                    return input;
                }

                // Display specific error message for invalid mobile number
                if (input.Length != 10)
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied mobile number is invalid, please try again. ");
                    Console.WriteLine("#####\n");
                }
                else if (!input.StartsWith("0"))
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied mobile number is invalid, please try again. ");
                    Console.WriteLine("#####\n");
                }
                else if (!input.All(char.IsDigit))
                {
                    Console.WriteLine("\n#####");
                    Console.WriteLine("#Error - Supplied mobile number is invalid, please try again. ");
                    Console.WriteLine("#####\n");
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Prompts the user to choose a specialty for the surgeon from a predefined list.
        /// </summary>
        /// <returns>The selected specialty as a string (e.g., "General Surgeon", "Orthopaedic Surgeon", etc.).</returns>
        private string ChooseSurgeonSpecialty()
        {
            while (true)
            {
                Console.WriteLine("Please choose your speciality: ");
                Console.WriteLine("1. General Surgeon");
                Console.WriteLine("2. Orthopaedic Surgeon");
                Console.WriteLine("3. Cardiothoracic Surgeon");
                Console.WriteLine("4. Neurosurgeon");

                Console.Write("Please enter a choice between 1 and 4: ");
                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        return "General Surgeon";
                    case "2":
                        return "Orthopaedic Surgeon";
                    case "3":
                        return "Cardiothoracic Surgeon";
                    case "4":
                        return "Neurosurgeon";
                    default:
                        Console.WriteLine("\n#####");
                        Console.WriteLine("#Error - Non-valid speciality type, please try again. ");
                        Console.WriteLine("#####\n");
                        System.Threading.Thread.Sleep(1000);
                        break;
                }
            }
        }

        /// <summary>
        /// Displays an error message to the user with added spacing and pauses for readability.
        /// </summary>
        /// <param name="message">The error message to be displayed.</param>
        private void DisplayError(string message)
        {
            Console.WriteLine(); // Add spacing before the error message
            Console.WriteLine(message);
            Console.WriteLine(); // Add spacing after the error message
            Thread.Sleep(1000); // Pause to give user time to see the error message
        }

        /// <summary>
        /// Checks if a given email is already registered in the users list.
        /// </summary>
        /// <param name="email">The email to check for existence.</param>
        /// <returns><c>true</c> if the email exists in the users list; otherwise, <c>false</c>.</returns>
        private bool EmailExists(string email)
        {
            foreach (var user in users)
            {
                if (user.Email == email)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a given staff ID is already registered for a staff member.
        /// </summary>
        /// <param name="staffId">The staff ID to check for existence.</param>
        /// <returns><c>true</c> if the staff ID exists; otherwise, <c>false</c>.</returns>
        private bool StaffIdExists(string staffId)
        {
            foreach (var user in users)
            {
                if (user is Staff staff && staff.StaffId == staffId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Prompts the user for a string input, with optional validation for specific input types like mobile numbers.
        /// </summary>
        /// <param name="prompt">The message to prompt the user for input.</param>
        /// <param name="errorMessage">The error message to display when the input is invalid.</param>
        /// <returns>The valid string input from the user, or <c>null</c> if the maximum attempts are exceeded.</returns>
        private string GetStringInput(string prompt, string errorMessage)
        {
            int attempts = 0;
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    // Assuming we want to validate mobile numbers as a special case
                    if (prompt.ToLower().Contains("mobile number"))
                    {
                        // Simple mobile number validation: only digits and must be 10 characters
                        if (input.All(char.IsDigit) && input.Length == 10)
                        {
                            return input;
                        }
                        else
                        {
                            Console.WriteLine("#####");
                            Console.WriteLine($"#Error - Supplied value is not an integer, please try again. ");
                            Console.WriteLine("#####");
                        }
                    }
                    else
                    {
                        return input;
                    }
                }

                Console.WriteLine($"#####");
                Console.WriteLine($"#Error - Supplied email is invalid, please try again. ");
                Console.WriteLine($"#####");

                // Optional: limit attempts for string inputs
                attempts++;
                if (attempts >= 3)
                {
                    Console.WriteLine("Too many invalid attempts. Returning to the main menu.");
                    return null; // Return null or signal the calling method to go back to the menu
                }
            }
        }

        /// <summary>
        /// Prompts the user for an integer input, validating it against specified conditions.
        /// </summary>
        /// <param name="prompt">The message to prompt the user for input.</param>
        /// <param name="errorMessage">The error message to display when the input is invalid.</param>
        /// <returns>The valid integer input from the user, or -1 if the maximum attempts are exceeded.</returns>
        private int GetIntInput(string prompt, string errorMessage)
        {
            int result;
            int attempts = 0; // Attempt counter
            while (true)
            {
                Console.WriteLine(prompt);  // Use WriteLine for cleaner formatting
                string input = Console.ReadLine();

                if (attempts >= 3)
                {
                    Console.WriteLine("Too many invalid attempts. Returning to the main menu.");
                    return -1; // Signal to return to the main menu
                }

                if (int.TryParse(input, out result) && result > 0)
                {
                    return result;
                }

                DisplayError(errorMessage);
                attempts++; // Increment on every invalid input
            }
        }

        /// <summary>
        /// Represents a room in the hospital.
        /// </summary>
        public class Room
        {
            /// <summary>
            /// Gets the room number.
            /// </summary>
            public int RoomNumber { get; private set; }
            public bool IsOccupied { get; set; } // Property to check if the room is occupied

            /// <summary>
            /// Initializes a new instance of the <see cref="Room"/> class.
            /// </summary>
            /// <param name="roomNumber">The room number to assign.</param>
            public Room(int roomNumber)
            {
                RoomNumber = roomNumber;
                IsOccupied = false;
            }
        }

        /// <summary>
        /// Represents a user of the hospital system.
        /// </summary>
        public abstract class User
        {
            /// <summary>
            /// Gets the name of the user.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the age of the user.
            /// </summary>
            public int Age { get; private set; }

            /// <summary>
            /// Gets the mobile number of the user.
            /// </summary>
            public string Mobile { get; private set; }

            /// <summary>
            /// Gets the email address of the user.
            /// </summary>
            public string Email { get; private set; }

            /// <summary>
            /// Gets the password of the user.
            /// </summary>
            public string Password { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="User"/> class.
            /// </summary>
            /// <param name="name">The name of the user.</param>
            /// <param name="age">The age of the user.</param>
            /// <param name="mobile">The mobile number of the user.</param>
            /// <param name="email">The email address of the user.</param>
            /// <param name="password">The password for the user's account.</param>
            protected User(string name, int age, string mobile, string email, string password)
            {
                Name = name;
                Age = age;
                Email = email;
                Mobile = mobile;
                Password = password;
            }

            public bool ChangePassword(string currentPassword, string newPassword)
            {
                if (Password == currentPassword)
                {
                    Password = newPassword;
                    return true;
                }
                return false;
            }

            public virtual void DisplayDetails()
            {
                Console.WriteLine($"Name: {Name}");
                Console.WriteLine($"Age: {Age}");
                Console.WriteLine($"Mobile phone: {Mobile}");  // Update here
                Console.WriteLine($"Email: {Email}");
            }
        }

        /// <summary>
        /// Represents a patient in the hospital.
        /// </summary>
        public class Patient : User
        {
            /// <summary>
            /// Gets or sets a value indicating whether the patient has undergone surgery.
            /// </summary>
            public bool HasSurgery { get; set; }

            /// <summary>
            /// Gets or sets the room number assigned to the patient. A value of -1 indicates no room assigned.
            /// </summary>
            public int RoomNumber { get; set; }

            /// <summary>
            /// Gets or sets the name of the surgeon assigned to the patient.
            /// </summary>
            public string SurgeonName { get; set; }

            /// <summary>
            /// Gets or sets the date and time of the patient's surgery.
            /// </summary>
            public string SurgeryDateTime { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the patient is currently checked in.
            /// </summary>
            public bool IsCheckedIn { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the patient is eligible for check-in.
            /// </summary>
            public bool IsEligibleForCheckIn { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Patient"/> class.
            /// </summary>
            /// <param name="name">The name of the patient.</param>
            /// <param name="age">The age of the patient.</param>
            /// <param name="mobile">The mobile number of the patient.</param>
            /// <param name="email">The email address of the patient.</param>
            /// <param name="password">The password for the patient's account.</param>
            public Patient(string name, int age, string mobile, string email, string password)
                : base(name, age, mobile, email, password)
            {
                RoomNumber = -1;  // Initialize with no assigned room
                SurgeonName = "Not assigned"; // Initialize with no assigned surgeon
                SurgeryDateTime = "Not scheduled"; // Initialize with no scheduled surgery
                IsCheckedIn = false;
                HasSurgery = false;
                IsEligibleForCheckIn = true;
            }

            public void AssignRoom(int roomNumber)
            {
                RoomNumber = roomNumber;
            }

            public void UnassignRoom()
            {
                RoomNumber = -1;  // Set to the unassigned value
            }

            // Method to check the patient into a room
            public void CheckIn(int roomNumber)
            {
                RoomNumber = roomNumber;
                IsCheckedIn = true;

            }

            public void CheckOut(Dictionary<(int, int), bool> roomFloorMapping)
            {
                if (RoomNumber != -1)
                {
                    // Keep room mapping unchanged to ensure room remains assigned until explicitly unassigned
                    IsCheckedIn = false;
                    HasSurgery = false; // Reset surgery status
                    IsEligibleForCheckIn = false;  // Patient is not eligible to check in again right after surgery
                }
            }


            public void MarkSurgeryComplete()
            {
                HasSurgery = true;
            }


            // Method to assign a surgeon and surgery time to the patient
            public void AssignSurgeon(string surgeonName, string dateTime)
            {
                SurgeonName = surgeonName;
                SurgeryDateTime = dateTime;
            }


            // Override DisplayDetails to show patient details including room and surgery info
            public override void DisplayDetails()
            {
                base.DisplayDetails();  // Call the base class to display common details

                if (RoomNumber != -1)
                {
                    Console.WriteLine($"Room Number: {RoomNumber}");
                }
                if (SurgeonName != "Not assigned")
                {
                    Console.WriteLine($"Surgeon: {SurgeonName}");
                }
                if (SurgeryDateTime != "Not scheduled")
                {
                    Console.WriteLine($"Surgery Date and Time: {SurgeryDateTime}");
                }
            }
        }

        /// <summary>
        /// Represents a manager responsible for managing a specific floor.
        /// </summary>
        public class Manager : BaseClass // Assuming BaseClass has a method to override
        {
            /// <summary>
            /// Gets or sets the floor number assigned to the manager.
            /// </summary>
            public int FloorNumber { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Manager"/> class.
            /// </summary>
            /// <param name="floorNumber">The floor number assigned to the manager.</param>
            public Manager(int floorNumber)
            {
                FloorNumber = floorNumber;
            }

            /// <summary>
            /// Displays the details of the manager, including the managed floor.
            /// </summary>
            public override void DisplayDetails()
            {
                base.DisplayDetails();
                Console.WriteLine($"Managing Floor: {FloorNumber}");
            }
        }

        public class BaseClass
        {
            public virtual void DisplayDetails()
            {
                Console.WriteLine("Displaying base details.");
            }
        }
        /// <summary>
        /// Represents a staff member in the hospital.
        /// </summary>
        public class Staff : User
        {
            /// <summary>
            /// Gets staff identification number.
            /// </summary>
            public string StaffId { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Staff"/> class.
            /// </summary>
            /// <param name="name">The name of the staff member.</param>
            /// <param name="age">The age of the staff member.</param>
            /// <param name="mobile">The mobile number of the staff member.</param>
            /// <param name="email">The email address of the staff member.</param>
            /// <param name="password">The password for the staff member's account.</param>
            /// <param name="staffId">The unique staff identification number.</param>
            public Staff(string name, int age, string mobile, string email, string password, string staffId)
                : base(name, age, mobile, email, password)
            {
                StaffId = staffId;
            }

            /// <summary>
            /// Displays the details of the staff member.
            /// </summary>
            public override void DisplayDetails()
            {
                base.DisplayDetails();
                // Console.WriteLine($"Staff ID: {StaffId}");
            }
        }

        /// <summary>
        /// Represents a floor manager in the hospital.
        /// </summary>
        public class FloorManager : Staff
        {
            /// <summary>
            /// Gets the floor number assigned to the floor manager.
            /// </summary>
            public int FloorNumber { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FloorManager"/> class.
            /// </summary>
            /// <param name="name">The name of the floor manager.</param>
            /// <param name="age">The age of the floor manager.</param>
            /// <param name="mobile">The mobile number of the floor manager.</param>
            /// <param name="email">The email address of the floor manager.</param>
            /// <param name="password">The password for the floor manager's account.</param>
            /// <param name="staffId">The unique staff identification number.</param>
            /// <param name="floorNumber">The floor assigned to the floor manager.</param>
            public FloorManager(string name, int age, string mobile, string email, string password, string staffId, int floorNumber)
                : base(name, age, mobile, email, password, staffId)
            {
                FloorNumber = floorNumber;
            }

            /// <summary>
            /// Displays the details of the floor manager, including the assigned floor.
            /// </summary>
            public override void DisplayDetails()
            {
                base.DisplayDetails();  // Calls the base class implementation to display name, age, email, etc.

                // No need to repeat Email or Mobile phone here as it's already done in base class.

                Console.WriteLine($"Staff ID: {StaffId}");   // Additional details specific to FloorManager
                Console.WriteLine($"Floor: {FloorNumber}.");
            }
        }

        /// <summary>
        /// Calculates the actual room number based on the floor number and room number within the floor.
        /// </summary>
        /// <param name="floorNumber">The floor number.</param>
        /// <param name="roomNumber">The room number within the floor (e.g., 1-10).</param>
        /// <returns>The actual room number in the hospital, accounting for all floors.</returns>
        private int CalculateActualRoomNumber(int floorNumber, int roomNumber)
        {
            // This method centralizes the logic for calculating the actual room number
            return (floorNumber - 1) * 10 + roomNumber;
        }

        /// <summary>
        /// Assigns a room to a patient.
        /// </summary>
        /// <param name="patient">The patient to whom the room is assigned.</param>
        /// <param name="room">The room number being assigned.</param>
        /// <param name="floorNumber">The floor on which the room is located.</param>
        public void AssignRoom(Patient patient, int room, int floorNumber)
        {
            if (room > 0) // Ensure the room number is valid
            {
                patient.CheckIn(room);
                Console.WriteLine($"Patient {patient.Name} has been assigned to room number {patient.RoomNumber} on floor {floorNumber}.");
            }
            else
            {
                Console.WriteLine($"Invalid room number for {patient.Name}. Cannot assign room.");
            }
        }

        /// <summary>
        /// Assigns a surgeon and schedules a surgery for a patient.
        /// </summary>
        /// <param name="patient">The patient undergoing surgery.</param>
        /// <param name="surgeon">The surgeon assigned to perform the surgery.</param>
        /// <param name="dateTime">The scheduled date and time for the surgery.</param>
        public void AssignSurgeon(Patient patient, Surgeon surgeon, string dateTime)
        {
            patient.AssignSurgeon(surgeon.Name, dateTime);  // This will update the surgeon and surgery date
            Console.WriteLine($"Surgeon {surgeon.Name} has been assigned to patient {patient.Name}.");
        }

        /// <summary>
        /// Represents a surgeon who is also a member of the hospital staff.
        /// </summary>
        public class Surgeon : Staff
        {
            /// <summary>
            /// Gets the specialty of the surgeon (e.g., General Surgeon, Cardiothoracic Surgeon).
            /// </summary>
            public string Specialty { get; private set; }

            /// <summary>
            /// Gets the list of upcoming surgeries assigned to the surgeon.
            /// </summary>
            public List<Patient> UpcomingSurgeries { get; private set; } = new List<Patient>();  // Store surgeries

            /// <summary>
            /// Initializes a new instance of the <see cref="Surgeon"/> class.
            /// </summary>
            /// <param name="name">The name of the surgeon.</param>
            /// <param name="age">The age of the surgeon.</param>
            /// <param name="mobile">The mobile number of the surgeon.</param>
            /// <param name="email">The email address of the surgeon.</param>
            /// <param name="password">The password for the surgeon's account.</param>
            /// <param name="staffId">The unique staff ID assigned to the surgeon.</param>
            /// <param name="specialty">The specialty of the surgeon.</param>
            public Surgeon(string name, int age, string mobile, string email, string password, string staffId, string specialty)
                : base(name, age, mobile, email, password, staffId)
            {
                Specialty = specialty;
            }

            /// <summary>
            /// Displays the schedule of upcoming surgeries assigned to the surgeon.
            /// </summary>
            public void SeeYourSchedule()
            {
                Console.WriteLine("\nYour schedule.");

                if (UpcomingSurgeries.Count == 0)
                {
                    Console.WriteLine("You do not have any patients assigned.");
                }
                else
                {
                    // Sort surgeries by scheduled time from earliest to latest
                    var sortedSurgeries = UpcomingSurgeries
                        .OrderBy(s => DateTime.ParseExact(s.SurgeryDateTime, "HH:mm dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                        .ToList();

                    foreach (var surgery in sortedSurgeries)
                    {
                        Console.WriteLine($"Performing surgery on patient {surgery.Name} on {surgery.SurgeryDateTime}");
                    }
                }
            }

            /// <summary>
            /// Adds a patient to the surgeon's list of upcoming surgeries if the patient is not already in the list.
            /// </summary>
            /// <param name="patient">The patient to add to the upcoming surgeries list.</param>
            public void AddSurgery(Patient patient)
            {
                if (!UpcomingSurgeries.Contains(patient))
                {
                    UpcomingSurgeries.Add(patient);
                }
            }

            /// <summary>
            /// Displays the surgeon's details, including their staff ID, specialty, and upcoming surgeries.
            /// </summary>
            public override void DisplayDetails()
            {
                base.DisplayDetails();
                Console.WriteLine($"Staff ID: {StaffId}");
                Console.WriteLine($"Speciality: {Specialty}");

                if (UpcomingSurgeries.Count > 0)
                {
                    Console.WriteLine("Upcoming surgeries:");
                    foreach (var patient in UpcomingSurgeries)
                    {
                        Console.WriteLine($"Patient: {patient.Name}, Surgery Date: {patient.SurgeryDateTime}");
                    }
                }
            }

            /// <summary>
            /// Static class that manages surgery-related functionalities for surgeons.
            /// </summary>
            public static class SurgeryManager
            {
                /// <summary>
                /// Displays the upcoming surgery schedule for the specified surgeon.
                /// </summary>
                /// <param name="surgeon">The surgeon whose schedule is to be displayed.</param>
                public static void SeeSchedule(Surgeon surgeon)
                {
                    if (surgeon.UpcomingSurgeries.Count == 0)
                    {
                        Console.WriteLine("\nYou do not have any scheduled surgeries.");
                        return;
                    }

                    // Debug before sorting
                    Console.WriteLine("\nDEBUG: Upcoming surgeries before sorting:");
                    foreach (var surgery in surgeon.UpcomingSurgeries)
                    {
                        Console.WriteLine($"Patient: {surgery.Name}, Surgery Date: {surgery.SurgeryDateTime}");
                    }

                    // Sort surgeries by scheduled time from earliest to latest
                    var sortedSurgeries = surgeon.UpcomingSurgeries
                        .OrderBy(s => DateTime.ParseExact(s.SurgeryDateTime, "HH:mm dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture))
                        .ToList();

                    // Debug after sorting
                    Console.WriteLine("\nDEBUG: Upcoming surgeries after sorting:");
                    foreach (var surgery in sortedSurgeries)
                    {
                        Console.WriteLine($"Patient: {surgery.Name}, Surgery Date: {surgery.SurgeryDateTime}");
                    }

                    Console.WriteLine("\nYour schedule:");
                    foreach (var surgery in sortedSurgeries)
                    {
                        Console.WriteLine($"Performing surgery on patient {surgery.Name} on {surgery.SurgeryDateTime}");
                    }
                }
            }

            /// <summary>
            /// Displays all upcoming surgeries scheduled for the surgeon.
            /// </summary>
            public void ShowUpcomingSurgeries()
            {
                Console.WriteLine($"Upcoming surgeries for {Name}:");
                foreach (var patient in UpcomingSurgeries)
                {
                    Console.WriteLine($"Patient: {patient.Name}, Surgery Date: {patient.SurgeryDateTime}");
                }
            }

            private Dictionary<int, List<int>> roomFloorMapping = new Dictionary<int, List<int>>();

            /// <summary>
            /// Populates the room-floor mapping dictionary with 10 rooms per floor for 6 floors.
            /// </summary>
            /// <remarks>
            /// Each room is assigned to all 6 floors, meaning that each room key in the dictionary will have a list
            /// of all floor numbers associated with it.
            /// </remarks>
            private void PopulateRoomFloorMapping()
            {
                // Assign all 10 rooms to all 6 floors
                for (int room = 1; room <= 10; room++)
                {
                    roomFloorMapping[room] = new List<int>();
                    for (int floor = 1; floor <= 6; floor++)
                    {
                        roomFloorMapping[room].Add(floor);
                    }
                }
            }
            /// <summary>
            /// Retrives a list of floors associated with the given room
            /// </summary>
            /// <param name="roomNumber"> The room number for which to get associated floors.</param>
            /// <returns>
            /// A list of floor numbers associated with the given room.
            /// Returns an empty list if the room number is invalid.
            /// </returns>
            private List<int> GetFloorsForRoom(int roomNumber)
            {
                if (roomFloorMapping.ContainsKey(roomNumber))
                {
                    return roomFloorMapping[roomNumber];
                }
                else
                {
                    return new List<int>(); // Return an empty list if the room number is invalid
                }
            }

            /// <summary>
            /// Performs surgery on a given patient.
            /// </summary>
            /// <param name="patient">The patient on whom surgery is being performed.</param>
            /// <remarks>
            /// If the surgery is successfully completed, the patient is removed from the list of upcoming surgeries.
            /// </remarks>
            public void PerformSurgery(Patient patient)
            {
                if (UpcomingSurgeries.Contains(patient))
                {
                    UpcomingSurgeries.Remove(patient); // Surgery completed, so remove from the upcoming list
                                                       //   Console.WriteLine($"Surgery for {patient.Name} has been successfully performed.");
                }
                else
                {
                    Console.WriteLine($"No scheduled surgery found for {patient.Name}.");
                }
            }
        }
    }
}