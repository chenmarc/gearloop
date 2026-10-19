namespace Gearloop.Data;

/// <summary>
/// Database schema. Each entry in <see cref="Migrations"/> moves the database up one version;
/// add new entries at the end and never edit old ones once shipped.
/// Enums are stored by name so reordering an enum never corrupts saved data.
/// </summary>
internal static class Schema
{
    public static readonly string[] Migrations =
    [
        """
        CREATE TABLE items (
            id               INTEGER PRIMARY KEY AUTOINCREMENT,
            name             TEXT    NOT NULL,
            brand            TEXT,
            model            TEXT,
            year             INTEGER,
            category         TEXT    NOT NULL,
            condition        TEXT,
            serial_number    TEXT,
            notes            TEXT,
            purchase_date    TEXT    NOT NULL,
            source           TEXT    NOT NULL,
            price_paid       TEXT    NOT NULL,
            sales_tax        TEXT    NOT NULL,
            shipping_in      TEXT    NOT NULL,
            listed_price     TEXT,
            listed_date      TEXT,
            sale_date        TEXT,
            sale_channel     TEXT,
            sale_price       TEXT,
            shipping_charged TEXT,
            shipping_cost    TEXT,
            fees             TEXT
        );

        CREATE TABLE item_expenses (
            id       INTEGER PRIMARY KEY AUTOINCREMENT,
            item_id  INTEGER NOT NULL REFERENCES items(id) ON DELETE CASCADE,
            date     TEXT    NOT NULL,
            type     TEXT    NOT NULL,
            amount   TEXT    NOT NULL,
            note     TEXT
        );
        CREATE INDEX ix_item_expenses_item ON item_expenses(item_id);

        CREATE TABLE item_photos (
            id       INTEGER PRIMARY KEY AUTOINCREMENT,
            item_id  INTEGER NOT NULL REFERENCES items(id) ON DELETE CASCADE,
            position INTEGER NOT NULL,
            path     TEXT    NOT NULL
        );
        CREATE INDEX ix_item_photos_item ON item_photos(item_id);

        CREATE TABLE general_expenses (
            id     INTEGER PRIMARY KEY AUTOINCREMENT,
            date   TEXT NOT NULL,
            type   TEXT NOT NULL,
            amount TEXT NOT NULL,
            note   TEXT
        );
        """,
    ];
}
