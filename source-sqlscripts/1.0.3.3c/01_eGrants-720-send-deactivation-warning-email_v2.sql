ALTER TABLE people_sent_warning
ADD [userid] [varchar](50) NULL;
							
UPDATE people_sent_warning
SET people_sent_warning.userid = p.userid
FROM people_sent_warning psw
JOIN people p ON p.person_id = psw.person_id