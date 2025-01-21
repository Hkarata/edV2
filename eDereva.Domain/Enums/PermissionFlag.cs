namespace eDereva.Domain.Enums;


[Flags]
public enum PermissionFlag
{
    None = 0,

    // User permissions
    ViewUsers = 1 << 0,
    EditUsers = 1 << 1,
    DeleteUsers = 1 << 2,
    ManageUsers = ViewUsers | EditUsers | DeleteUsers,

    // Venue permissions
    ViewVenues = 1 << 3,
    EditVenues = 1 << 4,
    DeleteVenues = 1 << 5,
    ManageVenues = ViewVenues | EditVenues | DeleteVenues,

    // Session permissions
    ViewSessions = 1 << 17,
    CreateSessions = 1 << 18,
    EditSessions = 1 << 19,
    DeleteSessions = 1 << 20,
    ManageSessions = ViewSessions | CreateSessions | EditSessions | DeleteSessions,

    // Question Bank permissions
    ViewQuestionBanks = 1 << 6,
    EditQuestionBanks = 1 << 7,
    DeleteQuestionBanks = 1 << 8,
    ManageQuestionBanks = ViewQuestionBanks | EditQuestionBanks | DeleteQuestionBanks,

    // Test permissions
    ViewTests = 1 << 9,
    EditTests = 1 << 10,
    DeleteTests = 1 << 11,
    ManageTests = ViewTests | EditTests | DeleteTests,

    // Booking permissions
    ViewBookings = 1 << 13,
    CreateBookings = 1 << 14,
    EditBookings = 1 << 15,
    DeleteBookings = 1 << 16,
    ManageBookings = ViewBookings | CreateBookings | EditBookings | DeleteBookings,

    // Special permissions
    ViewSoftDeletedData = 1 << 12,

    // Composite permissions
    Administrator = ManageUsers | ManageVenues | ManageSessions | ManageQuestionBanks |
                    ManageTests | ManageBookings | ViewSoftDeletedData
}