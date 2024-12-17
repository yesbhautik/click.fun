const express = require('express');
const router = express.Router();
const ClickData = require('../models/ClickData');
const logger = require('../utils/logger');
const authRoutes = require('./auth');
const jwt = require('jsonwebtoken');

// Mount auth routes
router.use('/auth', authRoutes);

// Health check endpoint
router.get('/health', (req, res) => {
    res.json({ status: 'ok' });
});

// Middleware to verify token
const verifyToken = async (req, res, next) => {
    try {
        const token = req.headers.authorization?.split(' ')[1];
        if (!token) {
            logger.warn('No token provided in request');
            return res.status(401).json({
                success: false,
                message: 'No token provided'
            });
        }

        const decoded = jwt.verify(token, process.env.JWT_SECRET);
        req.user = decoded;
        logger.debug(`Token verified for user: ${decoded.username}`);
        next();
    } catch (error) {
        logger.error('Token verification error:', error);
        return res.status(401).json({
            success: false,
            message: 'Invalid token'
        });
    }
};

// Save click data
router.post('/clicks', verifyToken, async (req, res) => {
    try {
        const { mouseClicks, keyboardPresses, username } = req.body;
        logger.debug(`Received click data: ${JSON.stringify(req.body)}`);
        
        // Verify the username matches the token
        if (username !== req.user.username) {
            logger.warn(`Username mismatch: ${username} vs ${req.user.username}`);
            return res.status(403).json({
                success: false,
                message: 'Username mismatch'
            });
        }

        const clickData = new ClickData({
            username,
            mouseClicks,
            keyboardPresses,
            timestamp: new Date()
        });

        await clickData.save();
        logger.info(`Click data saved for user: ${username} - Mouse: ${mouseClicks}, Keyboard: ${keyboardPresses}`);

        res.json({
            success: true,
            message: 'Click data saved successfully'
        });
    } catch (error) {
        logger.error('Error saving click data:', error);
        res.status(500).json({
            success: false,
            message: 'Error saving click data'
        });
    }
});

// View stats for a specific user
router.get('/view/stats', verifyToken, async (req, res) => {
    try {
        const { username } = req.query;
        
        // If username is provided, get stats for that user
        if (username) {
            // Verify the username matches the token
            if (username !== req.user.username) {
                logger.warn(`Unauthorized stats access attempt: ${username} by ${req.user.username}`);
                return res.status(403).json({
                    success: false,
                    message: 'Unauthorized to view other users stats'
                });
            }

            logger.info(`Fetching stats for user: ${username}`);
            
            // Get all-time stats
            const stats = await ClickData.find({ username })
                .sort({ timestamp: -1 })
                .lean();

            // Get last 24 hours stats
            const last24HourStats = await ClickData.getLast24HourStats(username);

            logger.debug(`Found ${stats.length} records for user ${username}`);

            const totalClicks = stats.reduce((sum, stat) => sum + (stat.mouseClicks || 0), 0);
            const totalPresses = stats.reduce((sum, stat) => sum + (stat.keyboardPresses || 0), 0);
            
            return res.json({
                success: true,
                data: {
                    username,
                    allTime: {
                        totalClicks,
                        totalPresses,
                        totalActions: totalClicks + totalPresses,
                        recordCount: stats.length
                    },
                    last24Hours: {
                        totalClicks: last24HourStats.totalClicks,
                        totalPresses: last24HourStats.totalPresses,
                        totalActions: last24HourStats.totalClicks + last24HourStats.totalPresses,
                        recordCount: last24HourStats.recordCount
                    },
                    recentStats: stats.slice(0, 10) // Return only the 10 most recent records
                }
            });
        }
        
        // If no username, get stats for all users (admin only)
        logger.info('Fetching stats for all users');
        
        const allStats = await ClickData.aggregate([
            {
                $group: {
                    _id: '$username',
                    totalClicks: { $sum: '$mouseClicks' },
                    totalPresses: { $sum: '$keyboardPresses' },
                    recordCount: { $sum: 1 },
                    lastUpdate: { $max: '$timestamp' }
                }
            },
            {
                $match: {
                    _id: { $ne: null }
                }
            },
            {
                $project: {
                    username: '$_id',
                    totalClicks: 1,
                    totalPresses: 1,
                    totalActions: { $add: ['$totalClicks', '$totalPresses'] },
                    recordCount: 1,
                    lastUpdate: 1,
                    _id: 0
                }
            },
            {
                $sort: { lastUpdate: -1 }
            }
        ]);

        logger.debug(`Found stats for ${allStats.length} users`);

        const summary = {
            totalUsers: allStats.length,
            totalClicks: allStats.reduce((sum, stat) => sum + (stat.totalClicks || 0), 0),
            totalPresses: allStats.reduce((sum, stat) => sum + (stat.totalPresses || 0), 0),
            totalRecords: allStats.reduce((sum, stat) => sum + (stat.recordCount || 0), 0)
        };
        summary.totalActions = summary.totalClicks + summary.totalPresses;

        res.json({
            success: true,
            data: {
                users: allStats,
                summary
            }
        });
    } catch (error) {
        logger.error('Error fetching stats:', error);
        res.status(500).json({
            success: false,
            message: 'Error fetching stats',
            error: error.message
        });
    }
});

module.exports = router; 