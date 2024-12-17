require('dotenv').config();
const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
const logger = require('./utils/logger');
const routes = require('./routes');

const app = express();
const port = process.env.PORT || 3000;

// Middleware
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(morgan('combined', { stream: { write: message => logger.info(message.trim()) } }));

// Redirect /test/count to /api/test/count
app.get('/test/count', (req, res) => {
    const newUrl = `/api/test/count${req.url.includes('?') ? req.url.substring(req.url.indexOf('?')) : ''}`;
    logger.info(`Redirecting from /test/count to ${newUrl}`);
    res.redirect(newUrl);
});

// Routes
app.use('/api', routes);

// Error handling middleware
app.use((err, req, res, next) => {
    logger.error(err.stack);
    res.status(500).json({
        success: false,
        message: 'Internal Server Error'
    });
});

// Connect to MongoDB
mongoose.connect(process.env.MONGODB_URI)
    .then(() => {
        logger.info('Connected to MongoDB');
        // Start server
        app.listen(port, () => {
            logger.info(`Server is running on port ${port}`);
        });
    })
    .catch((err) => {
        logger.error('MongoDB connection error:', err);
        process.exit(1);
    }); 