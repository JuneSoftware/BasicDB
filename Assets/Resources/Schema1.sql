CREATE TABLE IF NOT EXISTS settings (
	'key' VARCHAR PRIMARY KEY, 
	'value' VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS players (
	'id' INTEGER PRIMARY KEY,
	'name' VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS items (
	'id' INTEGER PRIMARY KEY,
	'name' VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS player_items (
	'player_id' INTEGER REFERENCES players(id),
	'item_id' INTEGER REFERENCES items(id),
	PRIMARY KEY (player_id, item_id)
);


INSERT INTO players(id, name) VALUES(1, 'Player1');
INSERT INTO players(id, name) VALUES(2, 'Player2');

INSERT INTO items(id, name) VALUES(1, 'Item1');
INSERT INTO items(id, name) VALUES(2, 'Item2');
INSERT INTO items(id, name) VALUES(3, 'Item3');
INSERT INTO items(id, name) VALUES(4, 'Item4');
INSERT INTO items(id, name) VALUES(5, 'Item5');

INSERT INTO player_items(player_id, item_id) VALUES(1, 1);
INSERT INTO player_items(player_id, item_id) VALUES(1, 2);
INSERT INTO player_items(player_id, item_id) VALUES(2, 2);
INSERT INTO player_items(player_id, item_id) VALUES(2, 3);
INSERT INTO player_items(player_id, item_id) VALUES(2, 5);

INSERT OR REPLACE INTO settings VALUES ('DB_VER','1');