-- Schema Version 2.0

INSERT INTO players(id, name) VALUES(3, 'Player3');

INSERT INTO items(id, name) VALUES(6, 'Item6');
INSERT INTO items(id, name) VALUES(7, 'Item7');
INSERT INTO items(id, name) VALUES(8, 'Item8');

INSERT INTO player_items(player_id, item_id) VALUES(1, 7);
INSERT INTO player_items(player_id, item_id) VALUES(1, 8);
INSERT INTO player_items(player_id, item_id) VALUES(2, 4);
INSERT INTO player_items(player_id, item_id) VALUES(3, 3);
INSERT INTO player_items(player_id, item_id) VALUES(3, 5);

INSERT OR REPLACE INTO settings VALUES ('DB_VER','2');