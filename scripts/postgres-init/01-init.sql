-- PostgreSQL initialization for Ncp.Admin platform scaffold
SELECT 'CREATE DATABASE ncp_admin'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ncp_admin')\gexec

SELECT 'PostgreSQL initialization completed successfully' AS message;