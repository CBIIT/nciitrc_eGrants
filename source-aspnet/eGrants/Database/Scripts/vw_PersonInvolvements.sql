CREATE VIEW vw_PersonInvolvements AS
SELECT e.appl_id, d.person_id, d.first_name, d.last_name, d.mi_name AS src_mi_name,
       c.email_addr, e.role_type_code, c.addr_type_code
FROM [IRDB].[dbo].[person_involvements_mv] e
JOIN [IRDB].[dbo].[persons_secure] d ON d.person_id = e.person_id
LEFT JOIN [IRDB].[dbo].[person_addresses_mv] c ON d.person_id = c.person_id
    AND c.addr_type_code = 'HOM' AND c.preferred_addr_code = 'Y'
WHERE e.role_type_code IN ('PI', 'MPI', 'CPI')